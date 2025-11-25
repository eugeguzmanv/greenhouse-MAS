import pandas as pd
import numpy as np

def generate_tomato_data(num_samples=2000):
    """
    Generates synthetic data for tomato classification.
    Half will be healthy (0), Half will be cut/infected (1).
    """
    data = []
    
    # ---------------------------------------------------------
    # PART 1: GENERATE HEALTHY TOMATOES (Label 0 - No Cut)
    # ---------------------------------------------------------
    # Logic: High redness, very low spots, smooth texture, healthy leaves
    for _ in range(num_samples // 2):
        sample = {
            'fruit_redness':   np.clip(np.random.normal(0.8, 0.1), 0, 1), # Mostly red
            'fruit_greenness': np.clip(np.random.normal(0.1, 0.1), 0, 1), # Low green
            'leaf_health':     np.clip(np.random.normal(0.85, 0.1), 0, 1),# Very healthy leaves
            'spot_count':      np.clip(np.random.normal(0.05, 0.05), 0, 1),# Almost no spots
            'spot_darkness':   np.random.uniform(0, 1),                   # Doesn't matter if no spots
            'surface_texture': np.clip(np.random.normal(0.1, 0.1), 0, 1), # Smooth
            'size':            np.clip(np.random.normal(0.6, 0.2), 0, 1), # Decent size
            'stem_brownness':  np.clip(np.random.normal(0.2, 0.1), 0, 1), # Greenish stem
            'label': 0  # <--- TARGET: DO NOT CUT
        }
        data.append(sample)

    # ---------------------------------------------------------
    # PART 2: GENERATE INFECTED TOMATOES (Label 1 - Cut)
    # ---------------------------------------------------------
    # We simulate 3 types of diseases so the NN learns different reasons to cut.
    for _ in range(num_samples // 2):
        disease_type = np.random.choice(['rot', 'mold', 'withered'])
        
        if disease_type == 'rot':
            # Rot: High spots, dark spots, wrinkled texture
            sample = {
                'fruit_redness':   np.clip(np.random.normal(0.6, 0.2), 0, 1),
                'fruit_greenness': np.clip(np.random.normal(0.2, 0.2), 0, 1),
                'leaf_health':     np.random.uniform(0, 1),
                'spot_count':      np.clip(np.random.normal(0.7, 0.2), 0, 1),  # HIGH SPOTS
                'spot_darkness':   np.clip(np.random.normal(0.8, 0.1), 0, 1),  # BLACK spots
                'surface_texture': np.clip(np.random.normal(0.8, 0.1), 0, 1),  # WRINKLED
                'size':            np.random.uniform(0.2, 0.8),
                'stem_brownness':  np.random.uniform(0, 1),
                'label': 1 # <--- TARGET: CUT
            }
        elif disease_type == 'mold':
            # Mold: White spots, often green
            sample = {
                'fruit_redness':   np.random.uniform(0, 1),
                'fruit_greenness': np.random.uniform(0, 1),
                'leaf_health':     np.clip(np.random.normal(0.3, 0.2), 0, 1),
                'spot_count':      np.clip(np.random.normal(0.6, 0.2), 0, 1),  # HIGH SPOTS
                'spot_darkness':   np.clip(np.random.normal(0.1, 0.1), 0, 1),  # WHITE spots
                'surface_texture': np.random.uniform(0, 1),
                'size':            np.random.uniform(0, 1),
                'stem_brownness':  np.random.uniform(0, 1),
                'label': 1 # <--- TARGET: CUT
            }
        elif disease_type == 'withered':
            # Withered: Dead leaves, brown stem, small size
            sample = {
                'fruit_redness':   np.random.uniform(0, 1),
                'fruit_greenness': np.random.uniform(0, 1),
                'leaf_health':     np.clip(np.random.normal(0.1, 0.1), 0, 1),  # DEAD LEAVES
                'spot_count':      np.random.uniform(0, 0.4),
                'spot_darkness':   np.random.uniform(0, 1),
                'surface_texture': np.clip(np.random.normal(0.9, 0.1), 0, 1),  # WRINKLED
                'size':            np.clip(np.random.normal(0.2, 0.1), 0, 1),  # TINY
                'stem_brownness':  np.clip(np.random.normal(0.9, 0.1), 0, 1),  # BROWN STEM
                'label': 1 # <--- TARGET: CUT
            }
            
        data.append(sample)

    # Convert to DataFrame and Shuffle rows so 0s and 1s are mixed
    df = pd.DataFrame(data)
    df = df.sample(frac=1).reset_index(drop=True) 
    
    return df

# Generate and Save
if __name__ == "__main__":
    df = generate_tomato_data(2000)
    df.to_csv('tomato_dataset.csv', index=False)
    print("Success! 'tomato_dataset.csv' created with", len(df), "rows.")
    print(df.head())