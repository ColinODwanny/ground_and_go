using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace ground_and_go.Models
{
    [Table("workout_log")]
    public class WorkoutLog : ObservableBaseModel, IEquatable<WorkoutLog>
    {

        string _logId = "-1";
        string _memberId = "-1";
        DateTime _dateTime;
        int? _workoutId = null;
        string _beforeJournal = "";
        string _afterJournal = "";

        [PrimaryKey("log_id")]
        public string LogId
        {
            get => _logId;
            set => SetProperty(ref _logId, value); 
        }

        [Column("member_id")]
        public string MemberId
        {
            get => _memberId;
            set => SetProperty(ref _memberId, value);
        }

        [Column("workout_id")]
        public int? WorkoutId
        {
            get => _workoutId;
            set => SetProperty(ref _workoutId, value);
        }

        [Column("before_journal")]
        public string BeforeJournal
        {
            get => _beforeJournal;
            set => SetProperty(ref _beforeJournal, value);
        }

        [Column("after_journal")]
        public string AfterJournal
        {
            get => _afterJournal;
            set => SetProperty(ref _afterJournal, value);
        }

        [Column("date")]
        public DateTime DateTime
        {
            get => _dateTime;
            set => SetProperty(ref _dateTime, value);
        }


        public override bool Equals(object? obj)
            => obj is WorkoutLog other && LogId == other.LogId;

        public bool Equals(WorkoutLog? other)
            => other is not null && LogId == other.LogId;

        public override int GetHashCode()
            => LogId.GetHashCode();
    }
}