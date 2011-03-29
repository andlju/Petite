using System;

namespace Petite
{
    public class ServiceKey
    {
        public string Name { get; set; }
        public Type ServiceType { get; set; }

        public ServiceKey(string name, Type serviceType)
        {
            Name = name;
            ServiceType = serviceType;
        }

        public bool Equals(ServiceKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.ServiceType, ServiceType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ServiceKey)) return false;
            return Equals((ServiceKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ ServiceType.GetHashCode();
            }
        }

        public static bool operator ==(ServiceKey left, ServiceKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServiceKey left, ServiceKey right)
        {
            return !Equals(left, right);
        }
    }
}