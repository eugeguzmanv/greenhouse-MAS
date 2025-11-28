import pandas as pd
import numpy as np

def generate_soft_label_data(num_samples=3000):
    print("Generating data with SOFT LABELS (Probabilities)...")
    data = []
    
    for _ in range(num_samples):
        # 1. Generate Random Attributes
        redness = np.random.uniform(0, 1)
        greenness = np.random.uniform(0, 1)
        spots = np.random.uniform(0, 1)
        spot_darkness = np.random.uniform(0, 1)
        texture = np.random.uniform(0, 1)
        leaf_health = np.random.uniform(0, 1)
        size = np.random.uniform(0, 1)
        stem_brownness = np.random.uniform(0, 1)
        
        # 2. Enhanced Logic Formula
        # More balanced weights to create better distribution
        # BAD indicators (increase score):
        bad_score = (
            spots * 1.2 +                    # High spots = bad
            spot_darkness * 0.8 +            # Dark spots = worse
            texture * 0.6 +                  # Rough texture = bad
            greenness * 0.5 +                # Still green = not ripe
            stem_brownness * 0.4             # Brown stem = aging
        )
        
        # GOOD indicators (decrease score):
        good_score = (
            redness * 1.0 +                  # Red = ripe/good
            leaf_health * 0.7 +              # Healthy leaves = good
            size * 0.3                       # Larger = more mature
        )
        
        raw_score = bad_score - good_score
        
        # 3. Convert to Probability using Sigmoid
        # Adjusted to center around 0 and use gentler slope (3 instead of 5)
        # This creates more gradual transitions and better distribution
        label_prob = 1 / (1 + np.exp(-3 * raw_score))
        
        # 4. Save the data
        data.append({
            'fruit_redness': redness,
            'fruit_greenness': greenness,
            'leaf_health': leaf_health,
            'spot_count': spots,
            'spot_darkness': spot_darkness,
            'surface_texture': texture,
            'size': size,
            'stem_brownness': stem_brownness,
            'label': label_prob 
        })

    return pd.DataFrame(data)

if __name__ == "__main__":
    df = generate_soft_label_data(3000)
    
    # Save the dataset
    df.to_csv('tomato_dataset.csv', index=False)
    
    print("\n✓ Success! Dataset generated.")
    print(f"\nLabel distribution:")
    print(f"  Mean: {df['label'].mean():.3f}")
    print(f"  Std:  {df['label'].std():.3f}")
    print(f"  Min:  {df['label'].min():.3f}")
    print(f"  Max:  {df['label'].max():.3f}")
    print(f"\n  Labels < 0.3 (good tomatoes): {(df['label'] < 0.3).sum()}")
    print(f"  Labels 0.3-0.7 (uncertain):   {((df['label'] >= 0.3) & (df['label'] <= 0.7)).sum()}")
    print(f"  Labels > 0.7 (bad tomatoes):  {(df['label'] > 0.7).sum()}")
    
    print("\nSample data:")
    print(df[['spot_count', 'fruit_redness', 'leaf_health', 'label']].head(10))