using System;

namespace PAAOM_Common.Models
{
    public struct Point3D : IEquatable<Point3D>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Point3D other)
        {
            return X.Equals(other.X) &&
                   Y.Equals(other.Y) &&
                   Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is Point3D other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Point3D left, Point3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point3D left, Point3D right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        public static double Distance(Point3D point1, Point3D point2)
        {
            double dx = point1.X - point2.X;
            double dy = point1.Y - point2.Y;
            double dz = point1.Z - point2.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}