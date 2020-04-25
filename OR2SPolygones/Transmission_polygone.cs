using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace or2s.Decodage
{
    public class Transmission_polygone
    {
        //permet de générer un byte[] contenant les polygones a afficher sur la carte
        public List<byte[]> Extraire_byte_from_polygone(string path)
        {

            FileStream fs = null;
            BinaryReader br = null;

            try
            {
                //on charge le fichier
                fs = new FileStream(path, FileMode.Open);
                br = new BinaryReader(fs);

                //grâce à cette liste, nous allons stocker nos polygones sélectionnés
                List<byte[]> liste_polygones = new List<byte[]>();

                //permet de stocker le nombre d'octet de stockage pour chaque polygone
                List<uint> liste_nb_octet_polygone = new List<uint>();
                //on lit le premier élément du fichier qui contient le nombre de polygones
                uint nb_polygone = br.ReadUInt32();

                //ensuite, on va récupérer les nombres d'octets pour chaque polygone.
                for (uint i = 0; i < nb_polygone; i++)
                {
                    liste_nb_octet_polygone.Add(br.ReadUInt32());
                }

                for (uint i = 0; i < nb_polygone; i++)
                {
                    liste_polygones.Add(br.ReadBytes((int)liste_nb_octet_polygone[(int)i]));//on ajoute le polygone dans la liste de stockage des polygones.                   
                }
                return liste_polygones;
            }
            catch (Exception err)
            {
                throw new IOException("Impossible de lire le fichier " + path, err);
            }
            finally
            {
                if (br != null)
                    br.Close();
                if (fs != null)
                    fs.Close();
            }
        }

        public List<byte[]> Extraire_byte_from_polygone(Stream stream)
        {
            BinaryReader br = null;

            try
            {
                br = new BinaryReader(stream);

                //grâce à cette liste, nous allons stocker nos polygones sélectionnés
                List<byte[]> liste_polygones = new List<byte[]>();

                //permet de stocker le nombre d'octet de stockage pour chaque polygone
                List<uint> liste_nb_octet_polygone = new List<uint>();
                //on lit le premier élément du fichier qui contient le nombre de polygones
                uint nb_polygone = br.ReadUInt32();

                //ensuite, on va récupérer les nombres d'octets pour chaque polygone.
                for (uint i = 0; i < nb_polygone; i++)
                {
                    liste_nb_octet_polygone.Add(br.ReadUInt32());
                }

                for (uint i = 0; i < nb_polygone; i++)
                {
                    liste_polygones.Add(br.ReadBytes((int)liste_nb_octet_polygone[(int)i]));//on ajoute le polygone dans la liste de stockage des polygones.                   
                }
                return liste_polygones;
            }
            catch (Exception err)
            {
                throw new IOException("Impossible de lire le stream", err);
            }
            finally
            {
                if (br != null)
                    br.Close();
            }
        }
    }
}
