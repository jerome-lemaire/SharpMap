using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace or2s.Decodage
{
    /// <summary>
    /// class correspondant à l'implémentation d'un objet contenant plusieurs CpolygoneGeo.
    /// On peut l'apparenter à une zone géographique contenant plusieurs polygones.
    /// Dans sa construction initiale, il repèsente une commune.
    /// </summary>
    public class CpolygonesGeo : IComparable<CpolygonesGeo> 
    {
        /// <summary>
        /// Permet de stocker des CpolygoneGeo.
        /// </summary>
        public CpolygoneGeo[] polyGeo = new CpolygoneGeo[0];
        /// <summary>
        /// Permet de stocker la liste des CpolygonesGeo pouvant être connexes avec cette objet.
        /// </summary>
        public CpolygonesGeo[] tab_poly_connexe;
        /// <summary>
        /// Permet de stocker l'indice du CpolygonesGeo.
        /// </summary>
        public Cidentifiant indice;
 
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public CpolygonesGeo()

        {

        }

        /// <summary>
        /// Constructeur avec un seul paramètre
        /// </summary>
        /// <param name="polyGeo"></param>
        public CpolygonesGeo(CpolygoneGeo[] polyGeo)
        {
            this.polyGeo = polyGeo;
        }

        /// <summary>
        /// Constructeur avec en paramètre un lecteur binaire et un entier
        /// </summary>
        /// <param name="br"></param>
        public CpolygonesGeo(BinaryReader br)
        {
            int i;
            try
            {
                if (br == null)
                    return;
                //le premier élément du binaryreader est un entier contenant le nombre de polygone
                this.polyGeo = new CpolygoneGeo[br.ReadInt32()];
                for (i = 0; i < polyGeo.Length; i++)
                {
                    CpolygoneGeo poly = new CpolygoneGeo(br);
                    polyGeo[i] = poly;
                }
            }
            catch (Exception err)
            {
                throw new Exception("erreur BinaryReader CpolygonesGeo", err);
            }
        }
        
        /// <summary>
        /// Fonction permettant de convertir les données en binaires
        /// </summary>
        /// <param name="bw"></param>
        public void WriteZone(BinaryWriter bw)
        {
            bw.Write(polyGeo.Length);
            for (int i = 0; i < polyGeo.Length; i++)
            {
                polyGeo[i].binaryPolygoneGeo(bw);
            }

        }

        /// <summary>
        /// Accesseur à la taille du tableau de polygones
        /// </summary>
        public int Length
        {
            get
            {
                return this.polyGeo.Length;
            }
        }

        /// <summary>
        /// Accesseur à une ligne du tableau de polygones
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public CpolygoneGeo this[int i]
        {
            get
            {
                return this.polyGeo[i];
            }
        }

        /// <summary>
        /// Accesseur permettant de retourner le centre du groupe de polygones
        /// </summary>
        public Cpoints centre
        {
            get
            {
                double y_min = polyGeo[0].get_y_min();
                double y_max = polyGeo[0].get_y_max();
                double x_min = polyGeo[0].get_x_min();
                double x_max = polyGeo[0].get_x_max();

                for (int i = 1; i < polyGeo.Length; i++)
                {
                    if (polyGeo[i].get_y_min() < y_min)
                        y_min = polyGeo[i].get_y_min();
                    if (polyGeo[i].get_y_max() > y_max)
                        y_max = polyGeo[i].get_y_max();
                    if (polyGeo[i].get_x_min() < x_min)
                        x_min = polyGeo[i].get_x_min();
                    if (polyGeo[i].get_x_max() > x_max)
                        x_max = polyGeo[i].get_x_max();
                }
                Cpoints pt = new Cpoints((y_max+y_min)/2,(x_max+x_min)/2);
                return pt;
            }
        }

        public Cpoints centroide
        {
            get
            {
                // Add the first point at the end of the array.
                int num_points = polyGeo[0].Coordonnee_polygone.Length;
                Cpoints[] pts = new Cpoints[num_points + 1];
                //Points.CopyTo(pts, 0);
                polyGeo[0].Coordonnee_polygone.CopyTo(pts, 0);
                pts[num_points] = polyGeo[0].Coordonnee_polygone[0];



                // Find the centroid.
                float lng = 0; // x
                float lat = 0; // y
                float second_factor;
                for (int i = 0; i < num_points; i++)
                {
                    second_factor = (float)(pts[i].lng * pts[i + 1].lat - pts[i + 1].lng * pts[i].lat);
                    lng += (float)((pts[i].lng + pts[i + 1].lng) * second_factor);
                    lat += (float)((pts[i].lat + pts[i + 1].lat) * second_factor);
                }

                // Divide by 6 times the polygon's area.
                float polygon_area = PolygonArea();
                lng /= (6 * polygon_area);
                lat /= (6 * polygon_area);

                // If the values are negative, the polygon is
                // oriented counterclockwise so reverse the signs.
                if (lng < 0)
                {
                    lng = -lng;
                    lat = -lat;
                }

                return new Cpoints(lat, lng);
            }
        }

        private float PolygonArea()  // Return the polygon's area in "square units."
        {
            // Return the absolute value of the signed area.
            // The signed area is negative if the polyogn is
            // oriented clockwise.
            return Math.Abs(SignedPolygonArea());
        }
        private float SignedPolygonArea()
        {
            // Add the first point at the end of the array.
            int num_points = polyGeo[0].Coordonnee_polygone.Length;
            Cpoints[] pts = new Cpoints[num_points + 1];
            polyGeo[0].Coordonnee_polygone.CopyTo(pts, 0);
            pts[num_points] = polyGeo[0].Coordonnee_polygone[0];

            // Get the areas.
            float area = 0;
            for (int i = 0; i < num_points; i++)
            {
                area += (float)((pts[i + 1].lng - pts[i].lng) *(pts[i + 1].lat + pts[i].lat) / 2);
            }

            // Return the result.
            return area;
        }

        /// <summary>
        /// Permet de d'affecter le contenu d'un CpolygonesGeo dans un autre
        /// </summary>
        /// <param name="polys"></param>
        public void affectation_contenu(CpolygonesGeo polys)
        {
            //this.polyGeo = polys.polyGeo;
            //this.tab_poly_connexe = polys.tab_poly_connexe;
            this.indice = polys.indice;
        }

        /// <summary>
        /// Permet de tester si deux CpolygonesGeo sont fusionnables ou non
        /// </summary>
        /// <param name="polys"></param>
        /// <returns></returns>
        public bool sont_fusionnables(CpolygonesGeo polys)
        {
            if (this.sont_susceptibles_connexes(polys))
            {
                foreach (CpolygoneGeo p in this.polyGeo)
                {
                    foreach (CpolygoneGeo p2 in polys.polyGeo)
                        if (p.ont_des_points_communs(p2))
                            return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Permet de comparer deux CpolygonesGeo par leur indice
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CpolygonesGeo other)
        {
            return this.indice.CompareTo(other.indice);
        }
                

        /// <summary>
        /// Permet de savoir si deux polygones sont susceptibles d'être connexes ou non
        /// </summary>
        /// <param name="polys"></param>
        /// <returns></returns>
        public bool sont_susceptibles_connexes(CpolygonesGeo polys)
        {
            if (polys == this)
                return false;
            foreach (CpolygonesGeo p in this.tab_poly_connexe)
                if (p.indice.identifiant == polys.indice.identifiant)
                //if (p == polys)
                    return true;
            foreach (CpolygonesGeo p in polys.tab_poly_connexe)
                if (p.indice.identifiant == this.indice.identifiant)
                    return true;

            return false;
        }
    }
}
