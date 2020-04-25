using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace or2s.Decodage
{
    /// <summary>
    /// Classe permettant de générer un objet point contenant les coordonnées GPS
    /// </summary>
    public class Cpoints
    {
        /// <summary>
        /// Permet de stocker la latitude du points
        /// </summary>
        public double latitude;
        /// <summary>
        /// Permet de stocker la longitude du points
        /// </summary>
        public double longitude;

        /// <summary>
        /// Constructeur par défaut 
        /// </summary>
        public Cpoints()
        {

        }

        /// <summary>
        /// Constructeur prenant en paramètre une latitude et une longitude
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public Cpoints(double longitude , double latitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        /// <summary>
        /// Constructeur par copie
        /// </summary>
        /// <param name="pt"></param>
        public Cpoints(Cpoints pt)
        {
            this.latitude = pt.latitude;
            this.longitude = pt.longitude;
        }

        /// <summary>
        /// Accesseur à la latitude d'un point
        /// </summary>
        public double lat
        {
            get
            {
                return this.latitude;
            }
        }

        /// <summary>
        /// Accesseur à la longitude d'un point
        /// </summary>
        public double lng
        {
            get
            {
                return this.longitude;
            }

        }

        /// <summary>
        /// Override de l'operateur == 
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator == (Cpoints c1, Cpoints c2)
        {
            if ((c1.lat == c2.lat) && (c1.lng == c2.lng))
                return true;
            else return false;
        }

        /// <summary>
        /// Override de l'operateur !=
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static bool operator !=(Cpoints c1, Cpoints c2)
        {
            if ((c1.lat == c2.lat) && (c1.lng == c2.lng))
                return false;
            else return true;
        }

        /// <summary>
        /// Override de la méthode equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Cpoints c = obj as Cpoints;

            if ((this.lat == c.lat) && (this.lng == c.lng))
                return true;
            else return false;
        }
    }
}
