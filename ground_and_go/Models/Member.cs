using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Dynamic;

namespace ground_and_go.Models
{
    [Table("members")]
    public class Member : ObservableBaseModel, IEquatable<Member>
    {
        string _memberId = "-1";
        string _first = "";
        string _last = "";
        string _email = "";

        [PrimaryKey("user_id")]
        public string MemberId
        {
            get => _memberId;
            set => SetProperty(ref _memberId, value);
        }

        [Column("first")]
        public string First
        {
            get => _first;
            set => SetProperty(ref _first, value);
        }

        [Column("last")]
        public string Last
        {
            get => _last;
            set => SetProperty(ref _last, value);
        }

        [Column("email")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        public override bool Equals(object? obj)
            => obj is Member other && MemberId == other.MemberId;

        public bool Equals(Member? other)
            => other is not null && MemberId == other.MemberId;

        public override int GetHashCode()
            => MemberId.GetHashCode();
    }


}