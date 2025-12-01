import pandas as pd
import numpy as np

def generate_soft_label_data(num_samples=3000):
    print("Generating TRULY BALANCED soft label data...")
    data = []
    
    for _ in range(num_samples):
        # Generate Random Attributes
        redness = np.random.uniform(0, 1)
        greenness = np.random.uniform(0, 1)
        spots = np.random.uniform(0, 1)
        spot_darkness = np.random.uniform(0, 1)
        texture = np.random.uniform(0, 1)
        leaf_health = np.random.uniform(0, 1)
        size = np.random.uniform(0, 1)
        stem_brownness = np.random.uniform(0, 1)
        
        # BALANCED weights - good and bad factors have equal total weight
        bad_score = (
            spots * 0.8 +              # Total bad weight = 3.0
            spot_darkness * 0.6 +
            texture * 0.5 +
            greenness * 0.6 +
            stem_brownness * 0.5
        )
        
        good_score = (
            redness * 1.2 +            # Total good weight = 3.0
            leaf_health * 1.0 +
            size * 0.8
        )
        
        # Raw score should be centered around 0
        raw_score = bad_score - good_score
        
        # Use gentler sigmoid with proper centering
        # Slope of 1.5 creates smooth transitions
        label_prob = 1 / (1 + np.exp(-1.5 * raw_score))
        
        # Add small noise to reach full 0.0-1.0 range
        noise = np.random.normal(0, 0.03)
        label_prob = label_prob + noise
        
        # Clip to valid range
        label_prob = np.clip(label_prob, 0.0, 1.0)
        
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
    
    # Save
    df.to_csv('tomato_dataset.csv', index=False)
    
    print("\n✓ Success! Dataset generated.")
    print(f"\nLabel distribution:")
    print(f"  Mean: {df['label'].mean():.3f} (target: ~0.5)")
    print(f"  Std:  {df['label'].std():.3f}")
    print(f"  Min:  {df['label'].min():.3f}")
    print(f"  Max:  {df['label'].max():.3f}")
    
    labels = df['label']
    print(f"\n  Distribution by range:")
    print(f"    Labels 0.0-0.2 (very good):   {(labels < 0.2).sum():4d} ({(labels < 0.2).sum()/30:.1f}%)")
    print(f"    Labels 0.2-0.4 (good):        {((labels >= 0.2) & (labels < 0.4)).sum():4d} ({((labels >= 0.2) & (labels < 0.4)).sum()/30:.1f}%)")
    print(f"    Labels 0.4-0.6 (uncertain):   {((labels >= 0.4) & (labels < 0.6)).sum():4d} ({((labels >= 0.4) & (labels < 0.6)).sum()/30:.1f}%)")
    print(f"    Labels 0.6-0.8 (bad):         {((labels >= 0.6) & (labels < 0.8)).sum():4d} ({((labels >= 0.6) & (labels < 0.8)).sum()/30:.1f}%)")
    print(f"    Labels 0.8-1.0 (very bad):    {(labels > 0.8).sum():4d} ({(labels > 0.8).sum()/30:.1f}%)")
    
    print("\n  Sample data:")
    print(df[['spot_count', 'fruit_redness', 'leaf_health', 'label']].head(10))
    
    print("\n  Extreme examples:")
    print("  Best tomatoes (lowest 5 labels):")
    print(df.nsmallest(5, 'label')[['fruit_redness', 'spot_count', 'leaf_health', 'label']])
    print("\n  Worst tomatoes (highest 5 labels):")
    print(df.nlargest(5, 'label')[['fruit_redness', 'spot_count', 'leaf_health', 'label']])