using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;

namespace ground_and_go.Models
{

    [Table("exercises")]
    public class Exercise : ObservableBaseModel, IEquatable<Exercise>
    {
        int _exerciseId = -1;

        string _name = "";

        string _videoLink = "";

        [PrimaryKey("exercise_id")]
        public int ExerciseId
        {
            get => _exerciseId;
            set => SetProperty(ref _exerciseId, value);
        }

        [Column("name")]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [Column("video_link")]
        public string VideoLink
        {
            get => _videoLink;
            set => SetProperty(ref _videoLink, value);
        }

        public override bool Equals(object? obj)
            => obj is Exercise other && ExerciseId == other.ExerciseId;

        public bool Equals(Exercise? other)
            => other is not null && ExerciseId == other.ExerciseId;

        public override int GetHashCode()
            => ExerciseId.GetHashCode();
    }
}