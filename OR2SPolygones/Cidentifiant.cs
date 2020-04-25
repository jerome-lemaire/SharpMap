using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace or2s.Decodage
{
    public class Cidentifiant : IComparable<Cidentifiant>
    {
        /// <summary>
        /// Permet de stocker l'id
        /// </summary>
        public int id;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Cidentifiant()
        {

        }

        /// <summary>
        /// Permet de comparer deux identifiants
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Cidentifiant other)
        {
            return this.id.CompareTo(other.id);
        }

        /// <summary>
        /// Constructeur prenant en paramètre un entier
        /// </summary>
        /// <param name="id"></param>
        public Cidentifiant(int id)
        {
            this.id = id;
        }

        /// <summary>
        /// Accesseur à un identifiant
        /// </summary>
        public int identifiant
        {
            get
            {
                return this.id;
            }
        }

        /// <summary>
        /// Permet d'accéder directement à l'attribut de la classe Cidentifiant
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static implicit operator int(Cidentifiant i)
        {
            return i.id;
        }
    }
}
