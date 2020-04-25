using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace or2s.Decodage
{
    public class CconstructeurDepuisBdd
    {
        //Contient l'ensemble des CpolygonesGeo
        public CpolygonesGeo[] tab_polygones_geo;
        //Contient l'ensemble des CpolygoneGeo
        public CpolygoneGeo[] liste_polygones;

        public CconstructeurDepuisBdd(List<Byte[]> reader, string nomCarte)
        {
            try
            {
                int indice_polygonegeo = 0;
                int indice_polygonesGeo = 0;
                //Permet de stocker l'ensemble des CpolygonesGeo. Sera ensuite transcrit en tableau dans tab_polygones_geo
                List<CpolygonesGeo> tampon_poly = new List<CpolygonesGeo>();
                //Permet de stocker l'ensemble des CpolygoneGeo. Sera ensuite transcrit en tableau dans liste_polygones
                List<CpolygoneGeo> liste_poly = new List<CpolygoneGeo>();

                foreach (byte[] tab in reader)
                {
                    if (tab.Length != 0)
                    {
                        //transcription de l'idatareader en binaryreader
                        MemoryStream stream = new MemoryStream(tab);
                        BinaryReader br = new BinaryReader(stream);
                        //end
                        //on genere l'element cpolygonesgeo correspondant à la ligne lu
                        CpolygonesGeo zone_geo = new CpolygonesGeo(br);
                        //on ajoute l'element a la liste de cpolygones_geo
                        tampon_poly.Add(zone_geo);
                        //on attribut un indice au cpolygonesgeo
                        zone_geo.indice = new Cidentifiant(indice_polygonesGeo);
                        //puis on incrémente l'indice pour le prochain cpolygonesgeo
                        ++indice_polygonesGeo;
                        //pour chaque cpolygonegeo présent dans le cpolygonesgeo, on va attribuer un indice
                        //et définir comme parent le cpolygonesgeo
                        foreach (CpolygoneGeo poly in zone_geo.polyGeo)
                        {
                            poly.indice = indice_polygonegeo;
                            poly.parent = zone_geo;
                            liste_poly.Add(poly);
                            ++indice_polygonegeo;
                        }
                        //on ferme le binaryreader et le memorystream
                        br.Close();
                        stream.Close();
                    }
                    else
                    {
                        CpolygonesGeo zone_geo = new CpolygonesGeo();
                        //on ajoute l'element a la liste de cpolygones_geo
                        tampon_poly.Add(zone_geo);
                        //on attribut un indice au cpolygonesgeo
                        zone_geo.indice = new Cidentifiant(indice_polygonesGeo);
                        //puis on incrémente l'indice pour le prochain cpolygonesgeo
                        ++indice_polygonesGeo;
                    }
                }

                //on transforme les deux listes en tableau que l'on affecte dans tab_polygones_geo et liste_polygones
                tab_polygones_geo = tampon_poly.ToArray();
                liste_polygones = liste_poly.ToArray();
            }
            catch (Exception err)
            {
                throw new Exception("erreur iDataReader " + nomCarte, err);
            }
        }

        public void tri()
        {
            //On créé 4 tableaux en parallèle contenant les polygones triés par x_max, y_max, x_min, y_min
            CpolygoneGeo[] tri_x_max = new CpolygoneGeo[liste_polygones.Length];
            CpolygoneGeo[] tri_x_min = new CpolygoneGeo[liste_polygones.Length];
            CpolygoneGeo[] tri_y_max = new CpolygoneGeo[liste_polygones.Length];
            CpolygoneGeo[] tri_y_min = new CpolygoneGeo[liste_polygones.Length];
            //on cree un cinquième tableau qui permettra de copier liste_polygones
            CpolygoneGeo[] tab_tri = new CpolygoneGeo[liste_polygones.Length];
            liste_polygones.CopyTo(tab_tri, 0);
            //on trie les 4 tableaux en fonction de leur x_min, x_max, y_min et y_max
            Parallel.Invoke(() => { tri_x_max = tri_tab_x_max(tri_x_max, tab_tri); },
                            () => { tri_x_min = tri_tab_x_min(tri_x_min, tab_tri); },
                            () => { tri_y_max = tri_tab_y_max(tri_y_max, tab_tri); },
                            () => { tri_y_min = tri_tab_y_min(tri_y_min, tab_tri); }
                           );

            //end
            DateTime start = DateTime.Now;
            //Recherche des polygones possibles d'être connexes pour chaque polygone
            Parallel.ForEach(liste_polygones, poly =>
            {
                //on crée 4 tableaux de boolean non trié qui permettront de stocker les résultats des tests
                Boolean[] poly_connexes_possibles_1 = new Boolean[liste_polygones.Length];
                Boolean[] poly_connexes_possibles_2 = new Boolean[liste_polygones.Length];
                Boolean[] poly_connexes_possibles_3 = new Boolean[liste_polygones.Length];
                Boolean[] poly_connexes_possibles_4 = new Boolean[liste_polygones.Length];

                if (liste_polygones.Length > 2)
                {
                    //permet de stocker l'indice des polygones
                    int ind_x_max;
                    int ind_x_min;
                    int ind_y_max;
                    int ind_y_min;

                    //on cherche le premier polygone dans tri_x_max dont le x_max est supérieur ou égal au x_min de poly 
                    ind_x_max = recherche_polygone_tri_x_max(tri_x_max, poly);

                    //boucle permettant de passer à vrai tous les polygones dont le x_max est supérieur ou égal au x_min de poly
                    for (int i = ind_x_max; i < liste_polygones.Length; i++)
                    {
                        poly_connexes_possibles_1[tri_x_max[i].indice] = true;
                    }

                    //on cherche le premier polygone dans tri_x_min dont le x_min est supérieur au x_max de poly
                    //ce coup-ci, on ne cherche pas de polygone dont le x_min est égal au x_max de poly car cela 
                    //l'exclurait du résultat (cf la boucle)
                    //poly_find_2= Array.Find(tri_x_min, element => element.x_min > poly.x_max);
                    ind_x_min = recherche_polygone_tri_x_min(tri_x_min, poly);

                    //boucle permettant de passer à vrai tous les polygones dont le x_min est inférieur ou égal au x_max de poly
                    //ce coup-ci, on parcourt le tableau dans le sens inverse 
                    for (int i = ind_x_min; i >= 0; i--)
                    {
                        poly_connexes_possibles_2[tri_x_min[i].indice] = true;
                    }

                    //on cherche le premier polygone dans tri_y_max dont le y_max est supérieur ou égal au y_min de poly
                    //poly_find_3 = Array.Find(tri_y_max, element => element.y_max >= poly.y_min);
                    ind_y_max = recherche_polygone_tri_y_max(tri_y_max, poly);

                    //boucle permettant de passer à vrai tous les polygones dont le y_max est supérieur ou égal au y_min de poly
                    for (int i = ind_y_max; i < liste_polygones.Length; i++)
                    {
                        poly_connexes_possibles_3[tri_y_max[i].indice] = true;
                    }

                    //on cherche le premier polygone dans tri_y_min dont le y_min est supérieur au y_max de poly
                    //ce coup-ci, on ne cherche pas de polygone dont le y_min est égal au y_max de poly car cela 
                    //l'exclurait du résultat (cf la boucle)
                    //poly_find_4 = Array.Find(tri_y_min, element => element.y_min > poly.y_max);
                    ind_y_min = recherche_polygone_tri_y_min(tri_y_min, poly);

                    //boucle permettant de passer à vrai tous les polygones dont le y_min est inférieur ou égal au y_max de poly
                    //ce coup-ci, on parcourt le tableau dans le sens inverse 
                    for (int i = ind_y_min; i >= 0; i--)
                    {
                        poly_connexes_possibles_4[tri_y_min[i].indice] = true;
                    }
                }
                else
                {
                    foreach (CpolygoneGeo polygone in liste_polygones)
                    {
                        //boucle permettant de passer à vrai tous les polygones dont le x_max est supérieur ou égal au x_min de poly
                        if (polygone.x_max >= poly.x_min)
                            poly_connexes_possibles_1[polygone.indice] = true;
                        else
                            poly_connexes_possibles_1[polygone.indice] = false;

                        //boucle permettant de passer à vrai tous les polygones dont le x_min est inférieur ou égal au x_max de poly
                        if (polygone.x_min <= poly.x_max)
                            poly_connexes_possibles_2[polygone.indice] = true;
                        else
                            poly_connexes_possibles_2[polygone.indice] = false;

                        //boucle permettant de passer à vrai tous les polygones dont le y_max est supérieur ou égal au y_min de poly
                        if (polygone.y_max >= poly.y_min)
                            poly_connexes_possibles_3[polygone.indice] = true;
                        else
                            poly_connexes_possibles_3[polygone.indice] = false;

                        //boucle permettant de passer à vrai tous les polygones dont le y_min est inférieur ou égal au y_max de poly
                        if (polygone.y_min <= poly.y_max)
                            poly_connexes_possibles_4[polygone.indice] = true;
                        else
                            poly_connexes_possibles_4[polygone.indice] = false;
                    }
                }

                //on crée un tableau permettant de stocker la liste des polygones sensibles d'être connexe avec poly
                List<CpolygonesGeo> liste_polygones_connexes_possibles = new List<CpolygonesGeo>();
                for (int i = 0; i < liste_polygones.Length; i++)
                {
                    if (poly_connexes_possibles_1[i] == true && poly_connexes_possibles_2[i] == true && poly_connexes_possibles_3[i] == true && poly_connexes_possibles_4[i] == true)
                        //vérifier la présence du parent dans la liste
                        liste_polygones_connexes_possibles.Add(liste_polygones[i].parent);
                }
                poly.polygones_connexes_possibles = liste_polygones_connexes_possibles.ToArray();
            });

            TimeSpan dur2 = DateTime.Now - start;

            //actuellement, nous avons une liste de de cpolygonesgeo qui peut être connexe pour chaque cpolygonegeo.
            //Nous allons remonter les listes d'éléments connexes aux cpolygonesgeo.
            foreach (CpolygonesGeo polys in tab_polygones_geo)
            {
                List<CpolygonesGeo> list_poly_connexe = new List<CpolygonesGeo>();
                foreach (CpolygoneGeo poly in polys.polyGeo)
                {
                    foreach (CpolygonesGeo p in poly.polygones_connexes_possibles)
                    {
                        if (!list_poly_connexe.Contains(p))
                            list_poly_connexe.Add(p);
                    }
                    poly.polygones_connexes_possibles = null;
                }
                polys.tab_poly_connexe = list_poly_connexe.ToArray();
            }
            //end
        }

        /// <summary>
        /// Crée un tableau trié par ordre croissant des x_max
        /// </summary>
        public CpolygoneGeo[] tri_tab_x_max(CpolygoneGeo[] tab, CpolygoneGeo[] tab_tri)
        {
            tab_tri.CopyTo(tab, 0);
            Array.Sort(tab, delegate(CpolygoneGeo poly1, CpolygoneGeo poly2)
            {
                return poly1.x_max.CompareTo(poly2.x_max);
            });
            return tab;
        }

        /// <summary>
        /// Crée un tableau trié par ordre croissant des x_min
        /// </summary>
        public CpolygoneGeo[] tri_tab_x_min(CpolygoneGeo[] tab, CpolygoneGeo[] tab_tri)
        {
            tab_tri.CopyTo(tab, 0);
            Array.Sort(tab, delegate(CpolygoneGeo poly1, CpolygoneGeo poly2)
            {
                return poly1.x_min.CompareTo(poly2.x_min);
            });
            return tab;
        }

        /// <summary>
        /// Crée un tableau trié par ordre croissant des y_max
        /// </summary>
        public CpolygoneGeo[] tri_tab_y_max(CpolygoneGeo[] tab, CpolygoneGeo[] tab_tri)
        {
            tab_tri.CopyTo(tab, 0);
            Array.Sort(tab, delegate(CpolygoneGeo poly1, CpolygoneGeo poly2)
            {
                return poly1.y_max.CompareTo(poly2.y_max);
            });
            return tab;
        }

        /// <summary>
        /// Crée un tableau par ordre croissant des y_min
        /// </summary>
        public CpolygoneGeo[] tri_tab_y_min(CpolygoneGeo[] tab, CpolygoneGeo[] tab_tri)
        {
            tab_tri.CopyTo(tab, 0);
            Array.Sort(tab, delegate(CpolygoneGeo poly1, CpolygoneGeo poly2)
            {
                return poly1.y_min.CompareTo(poly2.y_min);
            });
            return tab;
        }

        /// <summary>
        /// Recherche de façon dichotomique dans un tableau trié par x_max
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        public int recherche_polygone_tri_x_max(CpolygoneGeo[] tab, CpolygoneGeo poly)
        {
            int borne_inf = 0, borne_sup = tab.Length - 1;
            int moyenne = (borne_inf + borne_sup) / 2;
            int tampon_moyenne = 0;

            while (true)
            {

                if (moyenne == 0 || moyenne == tab.Length - 1)
                    return moyenne;
                if (moyenne == tampon_moyenne)
                    ++moyenne;
                if ((tab[moyenne].x_max >= poly.x_min) && (tab[moyenne - 1].x_max < poly.x_min))
                    return moyenne;
                else if (tab[moyenne].x_max < poly.x_min)
                    borne_inf = moyenne;
                else if (tab[moyenne].x_max >= poly.x_min)
                    borne_sup = moyenne;
                tampon_moyenne = moyenne;
                moyenne = (borne_inf + borne_sup) / 2;
            }
        }

        /// <summary>
        /// Recherche de façon dichotomique dans un tableau trié par x_min
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        public int recherche_polygone_tri_x_min(CpolygoneGeo[] tab, CpolygoneGeo poly)
        {
            int borne_inf = 0, borne_sup = tab.Length - 1;
            int moyenne = (borne_inf + borne_sup) / 2;
            int tampon_moyenne = 0;

            while (true)
            {
                if (moyenne == 0 || moyenne == tab.Length - 1)
                    return moyenne;
                if (moyenne == tampon_moyenne)
                    ++moyenne;
                if ((tab[moyenne].x_min > poly.x_max) && (tab[moyenne - 1].x_min <= poly.x_max))
                    return moyenne;
                else if (tab[moyenne].x_min <= poly.x_max)
                    borne_inf = moyenne;
                else if (moyenne + 1 != tab.Length)
                    if (tab[moyenne].x_min > poly.x_max && tab[moyenne + 1].x_min > poly.x_max)
                        borne_sup = moyenne;
                tampon_moyenne = moyenne;
                moyenne = (borne_inf + borne_sup) / 2;

            }
        }

        /// <summary>
        /// Recherche de façon dichotomique dans un tableau trié par y_max
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="poly"></param>
        public int recherche_polygone_tri_y_max(CpolygoneGeo[] tab, CpolygoneGeo poly)
        {
            int borne_inf = 0, borne_sup = tab.Length - 1;
            int moyenne = (borne_inf + borne_sup) / 2;
            int tampon_moyenne = 0;

            while (true)
            {
                if (moyenne == 0 || moyenne == tab.Length - 1)
                    return moyenne;
                if (moyenne == tampon_moyenne)
                    ++moyenne;
                if ((tab[moyenne].y_max >= poly.y_min) && (tab[moyenne - 1].y_max < poly.y_min))
                    return moyenne;
                else if (tab[moyenne].y_max < poly.y_min)
                    borne_inf = moyenne;
                else if (tab[moyenne].y_max >= poly.y_min)
                    borne_sup = moyenne;
                tampon_moyenne = moyenne;
                moyenne = (borne_inf + borne_sup) / 2;
            }
        }


        /// <summary>
        /// Recherche de façon dichotomique dans un tableau trié par y_min
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="poly"></param>
        /// <returns></returns>
        public int recherche_polygone_tri_y_min(CpolygoneGeo[] tab, CpolygoneGeo poly)
        {
            int borne_inf = 0, borne_sup = tab.Length - 1;
            int moyenne = (borne_inf + borne_sup) / 2;
            int tampon_moyenne = 0;
            while (true)
            {
                if (moyenne == 0 || moyenne == tab.Length - 1)
                    return moyenne;
                if (moyenne == tampon_moyenne)
                    ++moyenne;
                if ((tab[moyenne].y_min > poly.y_max) && (tab[moyenne - 1].y_min <= poly.y_max))
                    return moyenne;
                else if (tab[moyenne].y_min <= poly.y_max)
                    borne_inf = moyenne;
                else if (moyenne + 1 != tab.Length)
                    if (tab[moyenne].y_min > poly.y_max && tab[moyenne + 1].y_min > poly.y_max)
                        borne_sup = moyenne;
                tampon_moyenne = moyenne;
                moyenne = (borne_inf + borne_sup) / 2;
            }
        }
    }
}
