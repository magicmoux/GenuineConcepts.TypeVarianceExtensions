using System.Data.SqlTypes;

namespace System.TypeVarianceExtensions.Tests.TestTypes
{
    /// <summary>
    /// A marker interface for Range classes
    /// </summary>
    public interface IRange
    {
    }

    /// <summary>
    /// A generic class to handle ranges of values. The instances are immutable. To simplify
    /// manipulation, the lower bound is always included and the upper bound exluded
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Range<T>
        : IRange, IComparable<Range<T>>, IEquatable<Range<T>>
        where T : IComparable<T>, IEquatable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Range{T}"/> class.
        /// </summary>
        public Range()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Range{T}"/> class.
        /// </summary>
        /// <param name="gte">The range's minimum value.</param>
        /// <param name="lt">The range's maximum value (excluded).</param>
        public Range(T gte, T lt)
        {
            if (gte.CompareTo(lt) > 0)
            {
                Gte = lt;
                Lt = gte;
            }
            else
            {
                Gte = gte;
                Lt = lt;
            }
        }

        /// <summary>
        /// Gets or sets the gte.
        /// </summary>
        /// <value>The gte.</value>
        public T Gte { get; protected set; }

        /// <summary>
        /// Gets or sets the lt.
        /// </summary>
        /// <value>The lt.</value>
        public T Lt { get; protected set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// Compare l'instance actuelle avec un autre objet du même type et retourne un entier qui
        /// indique si l'instance actuelle précède ou suit un autre objet ou se trouve à la même
        /// position dans l'ordre de tri.
        /// </summary>
        /// <param name="other">Objet à comparer à cette instance.</param>
        /// <returns>
        /// Valeur qui indique l'ordre relatif des objets comparés.La valeur de retour a les
        /// significations suivantes :Valeur Signification Inférieure à zéro Cette instance précède
        /// <paramref name="other"/> dans l'ordre de tri. Zéro Cette instance se produit à la même
        /// position dans l'ordre de tri que <paramref name="other"/>. Supérieure à zéro Cette
        /// instance suit <paramref name="other"/> dans l'ordre de tri.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public int CompareTo(Range<T> other)
        {
            if (other == null) throw new ArgumentException();
            if (Gte.Equals(other.Gte) && Lt.Equals(other.Lt)) return 0;
            if (Gte.CompareTo(other.Gte) < 0) return -1;
            if (Gte.CompareTo(other.Gte) == 0 && Lt.CompareTo(other.Lt) < 0) return -1;
            return 1;
        }

        /// <summary>
        /// Indique si l'objet actuel est égal à un autre objet du même type.
        /// </summary>
        /// <param name="other">Objet à comparer avec cet objet.</param>
        /// <returns>
        /// true si l'objet en cours est égal au paramètre <paramref name="other"/> ; sinon, false.
        /// </returns>
        public virtual bool Equals(Range<T> other)
        {
            return CompareTo(other) == 0;
        }

        ////TODO définir ça comme méthode d'extension pour chaque IQueryProvider ILinqProvider ou l'équivalent RemotionLinq
        //// ? avec utilisation d'attribut pour identification par reflection ?
        //public static Expression<Func<Range<T>, Range<T>, bool>> IsCovered = (item1, item2) => item1.Gte.CompareTo(item2.Gte) >= 0 && item1.Lt.CompareTo(item2.Lt) <= 0;

        ////TODO définir directement dans un CSharpLinqQueryProvider or as a Fody Weaver plugin

        //public bool Covers(Range<T> other)
        //{
        //    return this.Gte.CompareTo(other.Gte) <= 0 && this.Lt.CompareTo(other.Lt) >= 0;
        //}

        //public bool IsCovered(Range<T> other)
        //{
        //    return Range<T>.Linq_IsCovered.Compile().Invoke(this, other);
        //}

        //public bool Intersects(Range<T> other)
        //{
        //    return (this.Gte.CompareTo(other.Lt) < 0 && other.Gte.CompareTo(this.Lt) < 0;
        //}
    }
}