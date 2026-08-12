using System;

namespace CoolapkUWP.Models.Update
{
    public readonly record struct SystemVersionInfo(int Major, int Minor, int Build, int Revision)
    {
        public int CompareTo(SystemVersionInfo other)
        {
            return Major != other.Major
                ? Major.CompareTo(other.Major)
                : Minor != other.Minor
                ? Minor.CompareTo(other.Minor)
                : Build != other.Build ? Build.CompareTo(other.Build) : Revision != other.Revision ? Revision.CompareTo(other.Revision) : 0;
        }

        public int CompareTo(object obj)
        {
            return obj is SystemVersionInfo other ? CompareTo(other) : throw new ArgumentException();
        }

        public static bool operator <(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) >= 0;

        public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";
    }
}
