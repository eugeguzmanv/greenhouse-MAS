import torch 
import torch.nn as nn

class TomatoHealthNet(nn.Module): 
    def __init__(self, input_features): 
        """
        Constructor to define the layers
        """
        super(TomatoHealthNet, self).__init__()
        self.layer1 = nn.Linear(in_features=input_features, out_features = 64) # layer 1 = input layer, 8 inputs into 64 neurons
        self.layer2 = nn.Linear(in_features=64, out_features=32)
        self.layer3 = nn.Linear(in_features=32, out_features = 16) # layer 2 = summarization from 64 to 32 neurons
        self.output_layer = nn.Linear(in_features = 16, out_features = 1) # output one single number
        
        #Actvation functions
        self.relu = nn.ReLU() #rectified linear unit for learning patterns
        self.dropout = nn.Dropout(0.2)  # Prevents overfitting
        self.sigmoid= nn.Sigmoid() # sigmoid function to output a number in [0.0-1.0]



    def forward(self, x): 
        """
        Args:
            x (Tensor): The input data (e.g., [Redness, Size, Spots...])
        Returns:
            Tensor: A probability between 0 and 1.
        """
        # Input -> layer1 -> relu
        # pass through the first matrix and filter with relu
        out = self.layer1(x)
        out = self.relu(out)
        out = self.dropout(out)

        # layer 1 -> layer 2 -> relu
        out = self.layer2(out)
        out = self.relu(out)
        out = self.dropout(out)
        
        #layer 3
        out = self.layer3(out)
        out = self.relu(out)

        # layer 2 -> output layer 
        out = self.output_layer(out)

        # apply sigmoid
        final_prediction = self.sigmoid(out)

        return final_prediction