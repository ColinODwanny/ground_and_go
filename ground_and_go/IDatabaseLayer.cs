//Colin O'Dwanny

using ground_and_go.Models;

namespace ground_and_go
{



    public interface IDatabaseLayer
    {
        //TODO: We will need a method that will retrieve the mindfulness exercises, but I don't know how that is going to be implimented yet. 
        //Will the meditations be stored as a link to a web resource? a byte array in the database?

        public Task loadDataOnLogin(int userId);
        public Task LoadJournalEntryHistory(int userId);
        public Task LoadWorkoutHistory(int userId);

        /// <summary>
        /// Retrieves an exercise from the DB
        /// </summary>
        /// <param name="exerciseId">unique identifier for an exercise</param>
        /// <returns>Exercise with the corresponding ID in the DB</returns>
        public Exercise GetExerciseById(int exerciseId);

        public Task UploadJournalEntry(String entry);
    }
}