using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace Application.Common.Extensions;

public static class GeometryExtensions
{
    private static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    /// <summary>
    /// Cria um Point geográfico a partir de latitude e longitude
    /// </summary>
    /// <param name="latitude">Latitude em graus decimais</param>
    /// <param name="longitude">Longitude em graus decimais</param>
    /// <returns>Point com coordenadas geográficas em WGS84</returns>
    public static Point CreatePoint(double latitude, double longitude)
    {
        // PostGIS usa longitude (X) e latitude (Y)
        return GeometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }

    /// <summary>
    /// Cria um Point geográfico a partir de latitude e longitude nullable
    /// </summary>
    /// <param name="latitude">Latitude em graus decimais</param>
    /// <param name="longitude">Longitude em graus decimais</param>
    /// <returns>Point com coordenadas geográficas ou null se alguma coordenada for null</returns>
    public static Point? CreatePoint(double? latitude, double? longitude)
    {
        if (!latitude.HasValue || !longitude.HasValue)
            return null;

        return CreatePoint(latitude.Value, longitude.Value);
    }

    /// <summary>
    /// Calcula a distância em metros entre dois pontos usando a fórmula haversine
    /// </summary>
    /// <param name="point1">Primeiro ponto</param>
    /// <param name="point2">Segundo ponto</param>
    /// <returns>Distância em metros</returns>
    public static double DistanceInMeters(Point point1, Point point2)
    {
        return point1.Distance(point2) * 111319.9; // Conversão aproximada de graus para metros
    }

    /// <summary>
    /// Verifica se um ponto está dentro de um raio em metros de outro ponto
    /// </summary>
    /// <param name="center">Ponto central</param>
    /// <param name="point">Ponto a verificar</param>
    /// <param name="radiusInMeters">Raio em metros</param>
    /// <returns>True se o ponto está dentro do raio</returns>
    public static bool IsWithinRadius(Point center, Point point, double radiusInMeters)
    {
        var distanceInDegrees = radiusInMeters / 111319.9; // Conversão aproximada de metros para graus
        return center.IsWithinDistance(point, distanceInDegrees);
    }
}
