using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace or2s.Decodage
{
    /// <summary>
    /// Objet contenant les coordonnées GPS d'un polygone
    /// </summary>
    public class CpolygoneGeo
    {
        /// <summary>
        /// Tableau permettant le stockage des Cpoints formant le polygone
        /// </summary>
        public Cpoints[] Coordonnee_polygone;
        /// <summary>
        /// Permet de stocker la coordonnée x_max du polygone
        /// </summary>
        public double x_max;
        /// <summary>
        /// Permet de stocker la coordonnée x_min du polygone
        /// </summary>
        public double x_min;
        /// <summary>
        /// Permet de stocker la coordonnée y_max du polygone
        /// </summary>
        public double y_max;
        /// <summary>
        /// Permet de stocker la coordonnée y_min du polygone
        /// </summary>
        public double y_min;
        /// <summary>
        /// Permet de stocker l'indice du polygone
        /// </summary>
        public int indice = -1;
        /// <summary>
        /// Permet de stocker la liste des CpolygonesGeo pouvant être connexes au polygone
        /// </summary>
        public CpolygonesGeo[] polygones_connexes_possibles;
        /// <summary>
        /// Permet de stocker l'adresse du CpolygonesGeo contenant le polygone
        /// </summary>
        public CpolygonesGeo parent;
        //public CpolygoneGeo[] polygones_connexes;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public CpolygoneGeo()
        { 
        
        }

        /// <summary>
        /// Constructeur avec en paramètre un fichier binaire
        /// </summary>
        /// <param name="br"></param>
        public CpolygoneGeo(BinaryReader br)
        {
            int i;
            try
            {
                //l'élément suivant dans le binaryreader est un entier contenant le nombre de points du polygone
                Coordonnee_polygone = new Cpoints[br.ReadInt32()];

                for (i = 0; i < Coordonnee_polygone.Length; i++)
                {
                    double x = br.ReadDouble();
                    double y = br.ReadDouble();
                    //puis nous avons plusieurs couples de double correspondant à un point 
                    Cpoints pt = new Cpoints(y, x);
                    Coordonnee_polygone[i] = pt;
                }
                initialisation_extremite();
            }
            catch (Exception err)
            {
                throw new Exception("erreur iDataReader ", err);
            }
        }
        
        /// <summary>
        /// Constructeur avec en paramètre un tableau de points et un indice
        /// </summary>
        /// <param name="Coordonnee_polygone"></param>
        /// <param name="indice"></param>
        public CpolygoneGeo(Cpoints[] Coordonnee_polygone, int indice)
        {
            this.Coordonnee_polygone = Coordonnee_polygone;
            initialisation_extremite();
            this.indice = indice;
        }

        /// <summary>
        /// Constructeur avec en paramètre un tableau de points 
        /// </summary>
        /// <param name="Coordonnee_polygone"></param>
        public CpolygoneGeo(Cpoints[] Coordonnee_polygone)
        {
            this.Coordonnee_polygone = Coordonnee_polygone;
            initialisation_extremite();
        }




        /// <summary>
        /// Function initialisant les variables x_max, y_max ,x_min, y_min
        /// </summary>
        public void initialisation_extremite()
        {
            this.x_max = this.get_x_max();
            this.y_max = this.get_y_max();
            this.x_min = this.get_x_min();
            this.y_min = this.get_y_min();
        }

        /// <summary>
        /// Méthode retournant un polygone en binaire
        /// </summary>
        /// <param name="bw"></param>
        public void binaryPolygoneGeo(BinaryWriter bw)
        {
            bw.Write(Coordonnee_polygone.Length);

            for (int i = 0; i < Coordonnee_polygone.Length; i++)
            {
                bw.Write(this[i].lat);
                bw.Write(this[i].lng);
            }
        }

        /// <summary>
        /// Accesseur à la taille d'un polygone
        /// </summary>
        public int Length
        {
            get
            {
                if (this.Coordonnee_polygone == null)
                    return 0;
                else
                    return this.Coordonnee_polygone.Length;
            }
        }

        /// <summary>
        /// Accesseur à une ligne du tableau des coordonnées
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Cpoints this[int i]
        {
            get
            {
                return this.Coordonnee_polygone[i];
            }
        }

        /// <summary>
        /// Retourne la plus petite valeur en x d'un point du polygone 
        /// </summary>
        /// <returns></returns>
        public double get_x_min()
        {
            double xmin = this[0].lng;
            for (int i = 1; i < this.Length; i++)
            {
                if (this[i].lng < xmin)
                    xmin = this[i].lng;
            }
            return xmin;
        }

        /// <summary>
        /// Retoune la plus grande valeur en x d'un point du polygone 
        /// </summary>
        /// <returns></returns>
        public double get_x_max()
        {
            double xmax = this[0].lng;
            for (int i = 1; i < this.Length; i++)
            {
                if (this[i].lng > xmax)
                    xmax = this[i].lng;
            }
            return xmax;
        }

        /// <summary>
        /// Retourne la plus petite valeur en y d'un point du polygone
        /// </summary>
        /// <returns></returns>
        public double get_y_min()
        {
            double ymin = this[0].lat;
            for (int i = 1; i < this.Length; i++)
            {
                if (this[i].lat < ymin)
                    ymin = this[i].lat;
            }
            return ymin;
        }

        /// <summary>
        /// Retourne la plus grande valeur en y d'un point du polygone
        /// </summary>
        /// <returns></returns>
        public double get_y_max()
        {
            double ymax = this[0].lat;
            for (int i = 1; i < this.Length; i++)
            {
                if (this[i].lat > ymax)
                    ymax = this[i].lat;
            }
            return ymax;
        }

        /// <summary>
        /// Fonction retournant le point précédent de celui en paramètre
        /// </summary>
        /// <param name="indice"></param>
        /// <returns></returns>
        public int precedent(int indice)
        {
            if (this.Coordonnee_polygone == null)
            {
                Exception err = new Exception("le tableau point est null");
                throw (err);
            }
            if (indice == 0)
            {
                if(this.Coordonnee_polygone[this.Length - 1] == this.Coordonnee_polygone[0])
                    return this.Length - 2;
                else
                    return this.Length - 1;
            }
            else
                return indice - 1;
           
            
        }

        /// <summary>
        /// Fonction retournant le point suivant de celui en paramètre
        /// </summary>
        /// <param name="indice"></param>
        /// <returns></returns>
        public int suivant(int indice)
        {
            if (this.Coordonnee_polygone == null)
            {
                Exception err = new Exception("le tableau de point est null");
                throw (err);
            }
            if (indice == this.Length - 1)
            { 
                if(this.Coordonnee_polygone[0] == this.Coordonnee_polygone[Length-1])
                    return 1;
                else
                    return 0;
            }
            else
                return indice + 1;
            
        }

        /// <summary>
        /// Fonction permettant de savoir si deux polygones ont des points en commun
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public Boolean ont_des_points_communs(CpolygoneGeo poly)
        {
            //tampon servant à mettre en mémoire le dernier point trouvé pour éviter d'avoir deux fois le même point
            Cpoints tampon_point = new Cpoints();
            int compteur = 0;
            if (poly.Coordonnee_polygone != null && this.Coordonnee_polygone != null)
            {
                foreach (Cpoints pt_tab1 in this.Coordonnee_polygone)
                {
                    foreach (Cpoints pt_tab2 in poly.Coordonnee_polygone)
                    {
                        if (pt_tab1 == pt_tab2 && pt_tab1 != tampon_point)
                        {
                            compteur += 1;
                            tampon_point = pt_tab1;
                        if (compteur == 2)
                            return true;
                        break;
                        }
                            
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Fonction retournant une liste de points communs entre deux polygones commençant et terminant par les deux extrêmes
        /// ainsi que la liste des points communs 
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        public Cpoints[] extremité_points_communs(CpolygoneGeo tab, out Cpoints[] points_commun)
        {
            //liste servant à stocker les points communs
            List<Cpoints> liste_points_communs = new List<Cpoints>();
            //liste servant à stocker les points communs extrêmes
            List<Cpoints> liste_points_communs_extremes = new List<Cpoints>();
            int indice1 = 0;
            int indice2 = 0;
            foreach (Cpoints pt_tab1 in this.Coordonnee_polygone)
            {
                foreach (Cpoints pt_tab2 in tab.Coordonnee_polygone)
                {
                    if (pt_tab2 == pt_tab1)
                    {
                        if (!liste_points_communs.Contains(pt_tab1))
                            liste_points_communs.Add(pt_tab1);
                        if ((tab[tab.precedent(indice2)] != this[this.precedent(indice1)])
                            && (tab[tab.precedent(indice2)] != this[this.suivant(indice1)]))
                        {
                            if (!liste_points_communs_extremes.Contains(pt_tab1))
                                liste_points_communs_extremes.Add(pt_tab1);
                        }
                        else if ((tab[tab.suivant(indice2)] != this[this.precedent(indice1)])
                            && (tab[tab.suivant(indice2)] != this[this.suivant(indice1)]))
                        {
                            if (!liste_points_communs_extremes.Contains(pt_tab1))
                                liste_points_communs_extremes.Add(pt_tab1);
                        }
                    }
                    indice2 += 1;
                }
                indice2 = 0;
                indice1 += 1;
            }
            points_commun = liste_points_communs.ToArray();
            return liste_points_communs_extremes.ToArray();
        }

        /// <summary>
        /// Fonction retournant si oui ou non un tableau contient un point
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool contien(Cpoints c)
        {
            foreach (Cpoints c1 in this.Coordonnee_polygone)
            {
                if (c1 == c)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Trouve l'indice d'un point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public int find_indice(Cpoints point)
        {
            foreach (Cpoints pt in this.Coordonnee_polygone)
            {
                if (pt == point)
                    return Array.IndexOf(this.Coordonnee_polygone,pt);
            }

            return -1; 
        }

        /// <summary>
        /// Permet de comparer la taille de deux polygones
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CpolygoneGeo other)
        {
            return this.Length.CompareTo(other.Length);
        }

        /// <summary>
        /// Permet de regarder si un polygone n'a que des points communs avec un autre
        /// </summary>
        /// <param name="poly2"></param>
        /// <returns></returns>
        public bool ont_que_des_points_communs(CpolygoneGeo poly2)
        {
            foreach (Cpoints pt in this.Coordonnee_polygone)
            {
                if (poly2.find_indice(pt) == -1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compare deux polygones de meme taille pour savoir si ils sont identiques ou non
        /// </summary>
        /// <param name="poly2"></param>
        /// <param name="pt_commun"></param>
        /// <returns></returns>
        public bool sont_identiques(CpolygoneGeo poly2, Cpoints pt_commun)
        {
            //
            int ind_poly1 = this.find_indice(pt_commun);
            int ind_poly2 = poly2.find_indice(pt_commun);
            if (this[this.suivant(ind_poly1)] == poly2[poly2.suivant(ind_poly2)])
            {
                for (int i = 0; i < this.Length; i++)
                {
                    if (!(this[this.suivant(ind_poly1)] == poly2[poly2.suivant(ind_poly2)]))
                        return false;
                }
                return true;
            }
            else if (this[this.suivant(ind_poly1)] == poly2[poly2.precedent(ind_poly2)])
            {
                for (int i = 0; i < this.Length; i++)
                {
                    if (!(this[this.suivant(ind_poly1)] == poly2[poly2.precedent(ind_poly2)]))
                        return false;
                }
                return true;
            }
            else
                return false;

        }
    }
}


