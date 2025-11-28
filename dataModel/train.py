import torch 
import torch.nn as nn
import torch.optim as optim
import pandas as pd
from sklearn.model_selection import train_test_split
from torch.utils.data import DataLoader, TensorDataset
#import our nn
from model import TomatoHealthNet

#---------------------Hyperparameters
#Learning rate = standard
LEARNING_RATE = 0.001

#Epochs= how many times we go through the entire dataset
EPOCHS = 100

#Batch size = how many tomatoes we look at after updating the brain 
BATCH_SIZE = 32 # a standard

# 1. Data loading
EXPECTED_COLUMNS = [
    'fruit_redness', 'fruit_greenness', 'leaf_health', 
    'spot_count', 'spot_darkness', 'surface_texture', 
    'size', 'stem_brownness'
]
print("Loading CSV data..")
df = pd.read_csv('tomato_dataset.csv')

# 2. separate features(x) and labels (y)
X = df[EXPECTED_COLUMNS].values 
y = df['label'].values

# 3. split into train (80%) and test (80%)
#random state = 42 ensures we get the same split every time 
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size = 0.2, random_state=42)

# 4. convert to pytorch tensors, using float32 for math
X_train = torch.tensor(X_train, dtype=torch.float32)
y_train = torch.tensor(y_train, dtype=torch.float32).unsqueeze(1) # shape: [N] -> [N, 1] formatting from array to tensor for pytorch

X_test = torch.tensor(X_test, dtype=torch.float32)
y_test = torch.tensor(y_test, dtype=torch.float32).unsqueeze(1)

# 5. Create DataLoader
"""The DataLoader is a conveyor belt. It automatically shuffles data 
   and hands it to the AI in chunks of 32 (BATCH_SIZE)."""
train_dataset = TensorDataset(X_train, y_train)
train_loader = DataLoader(train_dataset, batch_size=BATCH_SIZE, shuffle= True)

print("Data loaded successfully")

#6. Initialize the model
model = TomatoHealthNet(input_features=8)

#GPU check for cuda
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print(f"Training on: {device}")
model.to(device) # move the brain to the GPU

#7. Define Loss and optimizer
#BCELoss = Binary Cross Entropy Loss
# Loss and optimizer function for 0/1 answers
criterion = nn.BCELoss()

#Adam = a smart optimizer
#can use standard gradient descent but Adam adapts speed automatically
optimizer = optim.Adam(model.parameters(), lr = LEARNING_RATE, weight_decay=1e-5)
# Learning rate scheduler to reduce LR if stuck
scheduler = optim.lr_scheduler.ReduceLROnPlateau(
    optimizer, mode='min', factor=0.5, patience=10
)
#------------Training Loop
print("\nStarting training")
best_loss = float('inf')

for epoch in range(EPOCHS): 
    model.train() #trainng mode for the model
    running_loss = 0.0
    
    #Loop over ds in batches of 32
    for i, (inputs, labels) in enumerate(train_loader): 

        #1. move data to gpu
        inputs, labels = inputs.to(device), labels.to(device)

        #2. zero the gradients, clear the history of the previous batchs
        optimizer.zero_grad()

        #3. forward the pass (pass and process data to get a prediction)
        outputs = model(inputs)

        #4 calculate loss
        loss = criterion(outputs, labels)

        #5 backward the pass, calculate gradient for every weight
        loss.backward()

        # Gradient clipping to prevent exploding gradients
        torch.nn.utils.clip_grad_norm_(model.parameters(), max_norm=1.0)

        #6. optimization step, adjust the weights based on the gradients in 5
        optimizer.step()

        running_loss += loss.item()
    avg_loss = running_loss / len(train_loader)

    #print stats every 5 epochs
    if(epoch+1) % 5 == 0:
        
        print(f"Epoch[{epoch + 1}/{EPOCHS}], Loss : {avg_loss:.4f}")
    #save the best model
    if avg_loss < best_loss:
        best_loss = avg_loss
        torch.save(model.state_dict(), "tomato_nn_best.pth")

print("Training complete")

#-------------------EVALUATION

#test the model w/ the remaining 20% of the data 
model.eval() #evaluation mode
with torch.no_grad(): #turn off gradient calculation to save memory and time
    #move test data to gpu
    X_test_gpu = X_test.to(device)
    y_test_gpu = y_test.to(device)

    #get predictions
    outputs = model(X_test_gpu)

    # Test loss
    test_loss = criterion(outputs, y_test_gpu).item()
    print(f"\nTest Loss: {test_loss:.4f}")

    #convert probabilities to 0 or 1
    predicted = outputs.round()
    actuals = y_test_gpu.round() # Round soft labels (0.8 -> 1.0)

    #calc simple accuracy
    correct_count = predicted.eq(actuals).sum().item()
    accuracy = correct_count / y_test.shape[0]

    print(f"\nFinal Accuracy on test Set: {accuracy * 100:.2f}%")
    
    # Mean Absolute Error (better metric for soft labels)
    mae = torch.abs(outputs - y_test_gpu).mean().item()
    print(f"Mean Absolute Error: {mae:.4f}")

    #Show predictions vs actuals
    print("\nSample predictions:")
    for i in range(min(10, len(outputs))):
        print(f"Predicted: {outputs[i].item():.3f}, Actual: {y_test_gpu[i].item():.3f}")


    #---------------------SAve the model
    #Save state_dict, the learned weights and matrices
    torch.save(model.state_dict(), "tomato_nn.pth")
    print("Model saved as 'tomato_nn.pth'")

