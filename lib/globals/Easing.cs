using Godot;

public partial class Easing : GodotObject {
    // Scale, and clamp x to 0..1 range
    private static float ScaleClamp(float value, float min, float max) {
        return Mathf.Clamp((value - min) / (max - min), 0.0f, 1.0f);
    }

    public static float InvrsSmoothstep(float value, float min, float max) {
        float x = ScaleClamp(value, min, max);
        return 0.5f - Mathf.Sin(Mathf.Asin(1.0f - 2.0f*x)/3.0f);
    }

    public static float Smoothstep(float value, float min, float max) {
        float x = ScaleClamp(value, min, max);
        return x*x*(3.0f - 2.0f*x);
    }

    public static float Smootherstep(float value, float min, float max) {
        float x = ScaleClamp(value, min, max);
        return Mathf.Pow(x, 3) * (x * (6.0f*x - 15.0f) + 10.0f);
    }

    // decay should be 1~25, from slow to fast
    public static float ExpDecay(float value, float dest, float decay, float delta) {
        return dest+(value-dest)*Mathf.Exp(-decay*delta);
    }
}
