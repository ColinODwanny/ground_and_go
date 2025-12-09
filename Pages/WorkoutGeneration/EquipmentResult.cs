//Devlin Delegard
namespace ground_and_go.Pages.WorkoutGeneration;

public class EquipmentResult
{
    public bool HomeAccess { get; set; }
    public bool GymAccess { get; set; }
    public string WorkoutType { get; set; } = ""; // "Strength Training" or "Cardio"
}