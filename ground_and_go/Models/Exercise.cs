namespace ground_and_go.Models;

public class Exercise
{
    public int ExerciseId { get; set; }
    public string Name { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }

    public int MinRest { get; set; }
    public int MaxRest { get; set; }
    
    public string LinkToVideo { get; set; }

    public (int min, int max) RestRange
    {
        get
        {
            return (MinRest, MaxRest);
        }
    }

}