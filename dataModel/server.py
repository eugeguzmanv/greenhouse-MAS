import torch
import uvicorn 
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from model import TomatoHealthNet

#-------------Config 
#definition of the input strucuture
class TomatoInput(BaseModel): 
    fruit_redness: float
    fruit_greenness: float
    leaf_health: float
    spot_count: float
    spot_darkness: float
    surface_texture: float
    size: float
    stem_brownness: float
    x_coordinate: int
    y_coordinate: int


#---------Initialization
app = FastAPI()
# Allow cross-origin requests so Unity  can call this API.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
print("Initializing Neural Network...")

#1. create the architechture
model = TomatoHealthNet(input_features=8)

#2. load the weights
try: 
    model.load_state_dict(torch.load("tomato_nn_best.pth", map_location=torch.device('cpu')))
    model.eval() #ready mode
    print("Model loaded successfully")
except FileNotFoundError: 
    print("ERROR: tomato_model.pth not found. train the model first")



#---------------ENdpoint

#The function we will call from unity
@app.post("/predict")
def predict_tomato(data: TomatoInput): 
    # 1. Convert the input struct to a simple list, in the order used in training 
    features = [
        data.fruit_redness,
        data.fruit_greenness,
        data.leaf_health,
        data.spot_count,
        data.spot_darkness,
        data.surface_texture,
        data.size,
        data.stem_brownness
    ]
    

    #2. convert to pytorch tensor: 
    input_tensor = torch.tensor([features], dtype=torch.float32)

    #3 pass the input through the nn
    with torch.no_grad(): 
        prediction = model(input_tensor)
    
    #4 process result
    score = prediction.item() #extract float from tensor
    if score > 0.4 and score < 0.6: #decision of cutting
         cut_decision = "cut_plant"
    elif score > 0.6: 
        cut_decision = "cut_neighbors"
    else: 
        cut_decision = "dont_cut"

    #5 return JSON
    return {
        "x_coordinate": data.x_coordinate,
        "y_coordinate": data.y_coordinate,
        "probability": score,
        "cut_decision": cut_decision
    }


#------RUNNER
#(Levantar el server)
if __name__ == "__main__": 
    uvicorn.run(app, host= "0.0.0.0", port = 8000)