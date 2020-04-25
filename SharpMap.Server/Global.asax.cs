using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using SharpMapServer.Model;
using SharpMap.Layers;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using NetTopologySuite;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using or2s.Decodage;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

namespace SharpMapServer
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var gss = new NtsGeometryServices();
            var css = new SharpMap.CoordinateSystems.CoordinateSystemServices(
                new CoordinateSystemFactory(),
                new CoordinateTransformationFactory(),
                SharpMap.Converters.WellKnownText.SpatialReference.GetAllReferenceSystems());

            GeoAPI.GeometryServiceProvider.Instance = gss;
            SharpMap.Session.Instance
                .SetGeometryServices(gss)
                .SetCoordinateSystemServices(css)
                .SetCoordinateSystemRepository(css);

            string settingsfile = Server.MapPath("~/App_Data/settings.xml");
            XmlSerializer serializer = new XmlSerializer(typeof(SharpMapContext));
            if (!System.IO.File.Exists(settingsfile))
            {
                /*create default settings*/
                SharpMapContext ctx = new SharpMapContext();
                ctx.Capabilities = new WmsCapabilities()
                {
                    Title = "SharpMap Demo Server",
                    Abstract = "This is an example SharpMap server",
                    Keywords = "SharpMap,WMS"
                };

                ctx.Users = new List<User>();
                ctx.Users.Add(new User { UserName = "admin", Password = "sharpmap" });

                /*add default layer*/
                ctx.Layers = new List<SharpMapServer.Model.WmsLayer>();
                ctx.Layers.Add(new SharpMapServer.Model.WmsLayer() { Name = "States", Description = "Demo data over US States", Provider = "Shapefile", DataSource = "states.shp" });     
                
                FileStream fs = File.Create(settingsfile);
                serializer.Serialize(fs, ctx);
                fs.Close();
            }

            FileStream settingsStream = File.OpenRead(settingsfile);
            var settings = (SharpMapContext)serializer.Deserialize(settingsStream);
            settingsStream.Close();

            WMSServer.m_Capabilities = new SharpMap.Web.Wms.Capabilities.WmsServiceDescription
            {
                Abstract = settings.Capabilities.Abstract,
                AccessConstraints = settings.Capabilities.AccessConstraints,
                Fees = settings.Capabilities.Fees,
                Keywords = settings.Capabilities.Keywords.Split(','),
                LayerLimit = settings.Capabilities.LayerLimit,
                MaxHeight = settings.Capabilities.MaxHeight,
                MaxWidth = settings.Capabilities.MaxWidth,
                OnlineResource = settings.Capabilities.OnlineResource,
                Title = settings.Capabilities.Title
            };

            WMSServer.m_Map = new SharpMap.Map();
            foreach (var l in settings.Layers)
            {
                string ds = l.DataSource;
                if (!Path.IsPathRooted(ds))
                    ds = Server.MapPath(ds);

                VectorLayer lay = new VectorLayer(l.Name);

                switch (l.Provider)
                {
                    case "Shapefile":
                        lay.DataSource = new SharpMap.Data.Providers.ShapeFile(ds);
                        lay.SRID = 4326;
                        WMSServer.m_Map.Layers.Add(lay);
                        break;
                    case "OR2S":
                        using (System.Net.WebClient webClient = new System.Net.WebClient())
                        {
                            Transmission_polygone plg = new Transmission_polygone();
                            List<byte[]> polyBytes = plg.Extraire_byte_from_polygone(webClient.OpenRead(new Uri("https://calcul2.or2s.fr/requestMaps/api/polygones/" + l.DataSource)));
                            CconstructeurDepuisBdd polyOr2s = new CconstructeurDepuisBdd(polyBytes, l.Name);
                            lay.DataSource = new SharpMap.Data.Providers.GeometryProvider(processPolygonsOR2S(polyOr2s.tab_polygones_geo));
                            WMSServer.m_Map.Layers.Add(lay);
                        }
                        break;
                    case "geolocalisationOR2S":
                        GeometryFactory gf = new GeometryFactory();
                        List<IGeometry> pts = new List<IGeometry>();
                        pts.Add(gf.CreatePoint(new GeoAPI.Geometries.Coordinate(2.295753, 49.894067)));
                        pts.Add(gf.CreatePoint(new GeoAPI.Geometries.Coordinate(2.244, 49.87)));
                        lay.DataSource = new SharpMap.Data.Providers.GeometryProvider(pts);
                        WMSServer.m_Map.Layers.Add(lay);
                        break;
                }
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }

        private List<IPolygon> processPolygonsOR2S(CpolygonesGeo[] polygons)
        {
            GeometryFactory gf = new GeometryFactory();
            List<IPolygon> res = new List<IPolygon>();
            foreach (CpolygonesGeo multipoly in polygons)
            {
                List<IPolygon> innerPoly = new List<IPolygon>();
                foreach (CpolygoneGeo polygone in multipoly.polyGeo)
                {
                    Coordinate[] coords = polygone.Coordonnee_polygone.Select(pt => new Coordinate(pt.lat, pt.lng)).ToArray();
                    innerPoly.Add(gf.CreatePolygon(coords));
                }
                
                if (innerPoly.Count > 1)
                {
                    // Gestion des trous
                    ILinearRing shell = gf.CreateLinearRing(innerPoly.First().Coordinates);
                    ILinearRing[] holes = innerPoly.Skip(1).Select(p => gf.CreateLinearRing(p.Coordinates)).ToArray();
                    res.Add(gf.CreatePolygon(shell, holes));
                }
                else if (innerPoly.Count > 0) res.Add(innerPoly.First());
            }
            return res;
        }
    }
}
