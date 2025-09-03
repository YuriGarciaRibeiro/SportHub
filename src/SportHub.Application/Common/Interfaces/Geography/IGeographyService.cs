using NetTopologySuite.Geometries;
using Application.Common.Extensions;

namespace Application.Common.Interfaces.Geography;

/// <summary>
/// Interface para serviços geográficos
/// </summary>
public interface IGeographyService
{
    /// <summary>
    /// Calcula a distância em metros entre dois pontos
    /// </summary>
    double CalculateDistanceInMeters(Point point1, Point point2);

    /// <summary>
    /// Calcula a distância em metros entre duas coordenadas
    /// </summary>
    double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Verifica se um ponto está dentro de um raio específico de outro ponto
    /// </summary>
    bool IsWithinRadius(Point center, Point point, double radiusInMeters);

    /// <summary>
    /// Converte coordenadas para Point
    /// </summary>
    Point CreatePoint(double latitude, double longitude);

    /// <summary>
    /// Converte coordenadas nullable para Point
    /// </summary>
    Point? CreatePoint(double? latitude, double? longitude);
}

/// <summary>
/// Implementação do serviço geográfico
/// </summary>
public class GeographyService : IGeographyService
{
    public double CalculateDistanceInMeters(Point point1, Point point2)
    {
        return GeometryExtensions.DistanceInMeters(point1, point2);
    }

    public double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
    {
        var point1 = GeometryExtensions.CreatePoint(lat1, lon1);
        var point2 = GeometryExtensions.CreatePoint(lat2, lon2);
        return GeometryExtensions.DistanceInMeters(point1, point2);
    }

    public bool IsWithinRadius(Point center, Point point, double radiusInMeters)
    {
        return GeometryExtensions.IsWithinRadius(center, point, radiusInMeters);
    }

    public Point CreatePoint(double latitude, double longitude)
    {
        return GeometryExtensions.CreatePoint(latitude, longitude);
    }

    public Point? CreatePoint(double? latitude, double? longitude)
    {
        return GeometryExtensions.CreatePoint(latitude, longitude);
    }
}
