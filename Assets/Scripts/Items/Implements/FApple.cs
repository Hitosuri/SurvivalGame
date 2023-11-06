﻿namespace Assets.Scripts.Items.Implements {
    public class FApple : FoodItem {
        public override float HealthAmount => 10f;
        public override float HungerAmount => 20f;
        public override float ThirstAmount => 10f;
        public override string SpritePath => "Sprite/Id13_Raw_Apple";
        public override int Id => 13;
    }
}