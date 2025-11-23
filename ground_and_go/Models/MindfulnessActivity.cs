using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Dynamic;

namespace ground_and_go.Models
{
    [Table("mindfulness_activities")]
    public class MindfulnessActivity : ObservableBaseModel
    {
        int _id = 0;
        string _activityName = "";
        List<int> _associatedEmotions = new List<int>();
        string _youtubeLink = "";

        [PrimaryKey("id")]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        [Column("activity_name")]
        public string ActivityName
        {
            get => _activityName;
            set => SetProperty(ref _activityName, value);
        }

        [Column("associated_emotions")]
        public List<int> AssociatedEmotions
        {
            get => _associatedEmotions;
            set => SetProperty(ref _associatedEmotions, value);
        }

        [Column("youtube_link")]
        public string YoutubeLink
        {
            get => _youtubeLink;
            set => SetProperty(ref _youtubeLink, value);
        }

    }


}