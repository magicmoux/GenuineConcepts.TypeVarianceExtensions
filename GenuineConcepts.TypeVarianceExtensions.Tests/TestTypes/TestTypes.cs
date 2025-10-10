namespace System.TypeVarianceExtensions.Tests.TestTypes
{
     /// <summary>
    /// A class to handle range of DateTime instances with customizable precision.
    /// </summary>
    public class DateTimeRange
        : Range<DateTime>, IComparable<DateTimeRange>, IEquatable<DateTimeRange>
    {
        /// <summary>
        /// The infinity
        /// </summary>
        public static DateTimeRange Infinity = new DateTimeRange();

        /// <summary>
        /// Initializes a new instance of the DateTimeRange class.
        /// </summary>
        public DateTimeRange()
            : this(null, null)
        {
        }

        /// <summary>
        /// Constructs a DateRange from a DateTimeOffset and a precision until the next
        /// DateTimeOffset with same precision
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="precision"></param>
        public DateTimeRange(DateTime reference)
            : this(reference, reference)
        {
        }

        /// <summary>
        /// Constructs a DateRange from a DateTimeOffset and a duration (the duration can be negative)
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="duration"></param>
        /// <param name="precision"></param>
        public DateTimeRange(DateTime reference, TimeSpan duration)
            : this(reference, reference + duration)
        {
        }

        /// <summary>
        /// Constructs a DateRange from start and end dates
        /// </summary>
        /// <param name="gte"></param>
        /// <param name="lt"></param>
        /// <param name="precision"></param>
        public DateTimeRange(Nullable<DateTime> gte = null, Nullable<DateTime> lt = null)
            : base(gte ?? DateTime.MinValue, lt ?? DateTime.MaxValue)
        {
        }

        // /// <summary>
        // /// The Schedule instance the DateTimeRange was generated from if any
        // /// </summary>
        //public Schedule Schedule { get; internal set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <value>The duration.</value>
        public TimeSpan Duration
        {
            get
            {
                return Lt - Gte;
            }
        }

        /// <summary>
        /// Determines whether the specified object, is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance;
        /// otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as DateTimeRange);
        }

        /// <summary>
        /// Indique si l'objet actuel est égal à un autre objet du même type.
        /// </summary>
        /// <param name="other">Objet à comparer avec cet objet.</param>
        /// <returns>
        /// true si l'objet en cours est égal au paramètre <paramref name="other"/> ; sinon, false.
        /// </returns>
        public bool Equals(DateTimeRange other)
        {
            if (other == null) return false;
            return this.Gte == other.Gte && this.Lt == other.Lt;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Intersects(DateTimeRange other)
        {
            return this.Gte < other.Lt && other.Gte < this.Lt;
        }

        public bool Contains(DateTime date)
        {
            return this.Gte <= date && date < this.Lt;
        }

        public bool Contains(DateTimeOffset date)
        {
            return this.Gte <= date && date < this.Lt;
        }

        public int CompareTo(DateTimeRange other)
        {
            return this.Gte < other.Gte
                ? -1
                : this.Gte == other.Gte
                ? this.Lt < other.Lt
                ? -1
                : this.Lt == other.Lt
                ? 0
                : 1
                : 1;
        }
    }
}