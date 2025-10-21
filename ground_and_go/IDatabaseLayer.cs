//Colin O'Dwanny
namespace ground_and_go;
using ground_and_go.Models;

public interface IDatabaseLayer
{
    //TODO: We will need a method that will retrieve the mindfulness exercises, but I don't know how that is going to be implimented yet. 
    //Will the meditations be stored as a link to a web resource? a byte array in the database?

    /// <summary>
    /// Retrieves all Dates in which a journal entry was recorded
    /// </summary>
    /// <param name="UserId">Unique identifier for a user of ground and go</param>
    /// <returns>HashSet of the dates jouranl entries were recorded.  It is a Set becuase some dates may 
    /// have multiple journal entries, but that shouldn't matter for this method</returns>
    public HashSet<DateOnly> GetJournalEntryHistory(int UserId);

    /// <summary>
    /// Retrieves all Journal entries from the DB from a given date.
    /// </summary>
    /// <param name="UserId">Unique identifier for a user of ground and go</param>
    /// <param name="dateOfEntry">Date of the journal entries to retrieve</param>
    /// <returns>tuple of strings.  1st string is the journal entry before the workout,
    ///  and the 2nd is the journal entry afte the workout.</returns>
    public (string entryBeforeWorkout, string entryAfterWorkout) GetLoggedJournalEntriesByDate(int UserId, DateOnly dateOfEntry);

    /// <summary>
    /// Retrieves from the database all the dates in which a workout was completed
    /// </summary>
    /// <returns>Array of dates in which workouts were recorded</returns>
    public DateOnly[] GetWorkoutHistory(int UserId);

    /// <summary>
    /// Retrieves from the database the workout that was generated on the given date
    /// </summary>
    /// <param name="dateOfWorkout"> Date of the workout that you would like to retrieve </param>
    public Workout GetLoggedWorkoutByDate(int UserId, DateOnly dateOfWorkout);

    /// <summary>
    /// Retrieves an exercise from the DB
    /// </summary>
    /// <param name="exerciseId">unique identifier for an exercise</param>
    /// <returns>Exercise with the corresponding ID in the DB</returns>
    public Exercise GetExerciseById(int exerciseId);

    
}