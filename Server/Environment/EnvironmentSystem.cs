using System.Text.Json.Serialization;

namespace NeuroCity.Server.Environment;

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    Storm,
    Fog,
    Snow
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class SunState
{
    [JsonPropertyName("x")]
    public float X { get; set; }
    
    [JsonPropertyName("y")]
    public float Y { get; set; }
    
    [JsonPropertyName("z")]
    public float Z { get; set; }
    
    [JsonPropertyName("intensity")]
    public float Intensity { get; set; }
    
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#FFFFFF";
}

public class WeatherState
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Clear";
    
    [JsonPropertyName("intensity")]
    public float Intensity { get; set; }
    
    [JsonPropertyName("windSpeed")]
    public float WindSpeed { get; set; }
    
    [JsonPropertyName("windDirection")]
    public float WindDirection { get; set; }
    
    [JsonPropertyName("precipitation")]
    public float Precipitation { get; set; }
    
    [JsonPropertyName("visibility")]
    public float Visibility { get; set; } = 1.0f;
    
    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 20f;
}

public class EnvironmentState
{
    [JsonPropertyName("timeOfDay")]
    public float TimeOfDay { get; set; }

    [JsonPropertyName("dayLength")]
    public float DayLength { get; set; } = 600f;

    [JsonPropertyName("timeScale")]
    public float TimeScale { get; set; } = 1f;

    [JsonPropertyName("sun")]
    public SunState Sun { get; set; } = new();

    [JsonPropertyName("ambientColor")]
    public string AmbientColor { get; set; } = "#404060";

    [JsonPropertyName("ambientIntensity")]
    public float AmbientIntensity { get; set; } = 0.6f;

    [JsonPropertyName("skyColor")]
    public string SkyColor { get; set; } = "#87CEEB";

    [JsonPropertyName("fogColor")]
    public string FogColor { get; set; } = "#87CEEB";

    [JsonPropertyName("fogDensity")]
    public float FogDensity { get; set; } = 0.001f;

    [JsonPropertyName("weather")]
    public WeatherState Weather { get; set; } = new();

    [JsonPropertyName("isNight")]
    public bool IsNight { get; set; }

    [JsonPropertyName("season")]
    public string Season { get; set; } = "Summer";

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; } = 20f;

    [JsonPropertyName("humidity")]
    public float Humidity { get; set; } = 0.5f;

    [JsonPropertyName("windSpeed")]
    public float WindSpeed { get; set; }

    [JsonPropertyName("windDirection")]
    public float WindDirection { get; set; }

    public EnvironmentState()
    {
        TimeOfDay = 8.0f;
    }
}

public class EnvironmentSystem
{
    private EnvironmentState _state = new();
    private float _timeAccumulator;
    private readonly Random _random = new();
    private float _weatherTimer;
    private WeatherType _currentWeather = WeatherType.Clear;
    private Season _currentSeason = Season.Summer;
    private float _weatherTransitionSpeed = 0.5f;
    private float _targetFogDensity = 0.001f;
    private float _targetWeatherIntensity = 0f;
    private float _targetVisibility = 1.0f;
    private float _currentWeatherIntensity = 0f;
    private float _currentFogDensity = 0.001f;
    private float _sunAngle;
    private float _seasonTimer;
    private int _dayCount;

    public EnvironmentState State => _state;

    public void Initialize()
    {
        Console.WriteLine("[EnvironmentSystem] Initializing advanced day/night cycle...");
        Console.WriteLine($"[EnvironmentSystem] Starting time: {_state.TimeOfDay:F2}, Season: {_currentSeason}");
        UpdateEnvironment();
    }

    public void Update(float deltaTime)
    {
        var scaledDelta = deltaTime * _state.TimeScale;
        
        _timeAccumulator += scaledDelta;
        
        var timeIncrement = (_state.DayLength / 24f) * scaledDelta;
        _state.TimeOfDay += timeIncrement;
        
        if (_state.TimeOfDay >= 24f)
        {
            _state.TimeOfDay -= 24f;
            _dayCount++;
            OnNewDay();
        }

        _seasonTimer += scaledDelta;
        if (_seasonTimer >= 30f)
        {
            _seasonTimer = 0;
            ChangeSeason();
        }

        _weatherTimer += scaledDelta;
        if (_weatherTimer >= 15f)
        {
            _weatherTimer = 0;
            ChangeWeather();
        }

        UpdateSunPosition();
        UpdateLighting();
        UpdateWeatherEffects(scaledDelta);
        UpdateWind(scaledDelta);
    }

    private void OnNewDay()
    {
        Console.WriteLine($"[EnvironmentSystem] Day {_dayCount} begins - Weather: {_currentWeather}, Season: {_currentSeason}");
    }

    private void UpdateSunPosition()
    {
        var hour = _state.TimeOfDay;
        
        _sunAngle = (hour - 6) / 12f * MathF.PI;
        
        var sunDistance = 500f;
        _state.Sun.X = MathF.Cos(_sunAngle) * sunDistance;
        _state.Sun.Y = MathF.Sin(_sunAngle) * sunDistance;
        _state.Sun.Z = 0;

        _state.IsNight = hour < 5.5f || hour > 20.5f;
    }

    private void UpdateLighting()
    {
        var hour = _state.TimeOfDay;
        var seasonModifier = _currentSeason switch
        {
            Season.Summer => 1.1f,
            Season.Winter => 0.8f,
            Season.Spring => 1.0f,
            Season.Autumn => 0.95f,
            _ => 1.0f
        };

        if (hour >= 5 && hour < 7)
        {
            var t = (hour - 5) / 2f;
            var sunsetT = MathF.Sin(t * MathF.PI / 2);
            _state.Sun.Intensity = MathF.Lerp(0.05f, 0.8f * seasonModifier, t) * (1 - _currentWeatherIntensity * 0.7f);
            _state.AmbientIntensity = MathF.Lerp(0.1f, 0.4f, t);
            _state.AmbientColor = LerpColorHex("#0a0a1a", "#303050", t);
            _state.SkyColor = LerpColorHex("#0a0a1a", "#ff7e47", sunsetT);
            _state.FogColor = LerpColorHex("#0a0a1a", "#ff8866", sunsetT);
            _state.FogDensity = MathF.Lerp(0.015f, 0.004f, t) + _currentFogDensity;
        }
        else if (hour >= 7 && hour < 9)
        {
            var t = (hour - 7) / 2f;
            _state.Sun.Intensity = MathF.Lerp(0.8f, 1.3f * seasonModifier, t) * (1 - _currentWeatherIntensity * 0.7f);
            _state.AmbientIntensity = MathF.Lerp(0.4f, 0.6f, t);
            _state.AmbientColor = LerpColorHex("#303050", "#606080", t);
            _state.SkyColor = LerpColorHex("#ff7e47", "#87CEEB", t);
            _state.FogColor = "#87CEEB";
            _state.FogDensity = MathF.Lerp(0.004f, 0.001f, t) + _currentFogDensity;
        }
        else if (hour >= 9 && hour < 17)
        {
            var middayBoost = hour >= 11 && hour <= 14 ? 0.1f : 0f;
            _state.Sun.Intensity = (1.3f * seasonModifier + middayBoost) * (1 - _currentWeatherIntensity * 0.7f);
            _state.AmbientIntensity = 0.6f;
            _state.AmbientColor = "#606080";
            
            if (_currentSeason == Season.Summer)
            {
                _state.SkyColor = "#87CEEB";
                _state.FogColor = "#87CEEB";
            }
            else if (_currentSeason == Season.Winter)
            {
                _state.SkyColor = "#b8d4e8";
                _state.FogColor = "#c8dce8";
            }
            
            _state.FogDensity = 0.001f + _currentFogDensity;
        }
        else if (hour >= 17 && hour < 19)
        {
            var t = (hour - 17) / 2f;
            _state.Sun.Intensity = MathF.Lerp(1.3f, 0.7f * seasonModifier, t) * (1 - _currentWeatherIntensity * 0.7f);
            _state.AmbientIntensity = MathF.Lerp(0.6f, 0.35f, t);
            _state.AmbientColor = LerpColorHex("#606080", "#504030", t);
            _state.SkyColor = LerpColorHex("#87CEEB", "#ff8866", t);
            _state.FogColor = LerpColorHex("#87CEEB", "#ff7755", t);
            _state.FogDensity = MathF.Lerp(0.001f, 0.005f, t) + _currentFogDensity;
        }
        else if (hour >= 19 && hour < 21)
        {
            var t = (hour - 19) / 2f;
            _state.Sun.Intensity = MathF.Lerp(0.7f, 0.05f, t) * (1 - _currentWeatherIntensity * 0.7f);
            _state.AmbientIntensity = MathF.Lerp(0.35f, 0.1f, t);
            _state.AmbientColor = LerpColorHex("#504030", "#0a0a1a", t);
            _state.SkyColor = LerpColorHex("#ff8866", "#1a1a3a", t);
            _state.FogColor = LerpColorHex("#ff7755", "#151530", t);
            _state.FogDensity = MathF.Lerp(0.005f, 0.012f, t) + _currentFogDensity;
        }
        else
        {
            _state.Sun.Intensity = 0.05f * (1 - _currentWeatherIntensity * 0.5f);
            _state.AmbientIntensity = 0.1f;
            _state.AmbientColor = "#0a0a1a";
            _state.SkyColor = "#0a0a1a";
            _state.FogColor = "#0a0a15";
            _state.FogDensity = 0.012f + _currentFogDensity;
        }

        _state.Temperature = CalculateTemperature();
    }

    private float CalculateTemperature()
    {
        var baseTemp = _currentSeason switch
        {
            Season.Summer => 28f,
            Season.Winter => 2f,
            Season.Spring => 18f,
            Season.Autumn => 14f,
            _ => 20f
        };

        var hour = _state.TimeOfDay;
        var timeModifier = hour >= 6 && hour <= 14 ? 
            (hour - 6) / 8f * 8f : 
            14f - (hour - 14) / 10f * 8f;

        var weatherModifier = _currentWeatherIntensity * -5f;
        
        return baseTemp + timeModifier + weatherModifier;
    }

    private void UpdateWeatherEffects(float deltaTime)
    {
        _currentWeatherIntensity = MathF.Lerp(_currentWeatherIntensity, _targetWeatherIntensity, _weatherTransitionSpeed * deltaTime);
        _currentFogDensity = MathF.Lerp(_currentFogDensity, _targetFogDensity, _weatherTransitionSpeed * deltaTime);

        _state.Weather.Intensity = _currentWeatherIntensity;
        _state.Weather.Visibility = _targetVisibility;
        
        switch (_currentWeather)
        {
            case WeatherType.Clear:
                _state.Weather.Precipitation = 0f;
                break;
            case WeatherType.Cloudy:
                _state.Weather.Precipitation = 0f;
                break;
            case WeatherType.Rain:
                _state.Weather.Precipitation = MathF.Lerp(_state.Weather.Precipitation, 0.7f, deltaTime * 2);
                break;
            case WeatherType.Storm:
                _state.Weather.Precipitation = MathF.Lerp(_state.Weather.Precipitation, 1f, deltaTime * 3);
                break;
            case WeatherType.Fog:
                _state.FogDensity = 0.025f;
                break;
            case WeatherType.Snow:
                _state.Weather.Precipitation = MathF.Lerp(_state.Weather.Precipitation, 0.5f, deltaTime);
                break;
        }
    }

    private void UpdateWind(float deltaTime)
    {
        _state.WindDirection += (_random.NextSingle() - 0.5f) * 0.1f * deltaTime;
        
        var baseWindSpeed = _currentWeather switch
        {
            WeatherType.Clear => 2f,
            WeatherType.Cloudy => 5f,
            WeatherType.Rain => 10f,
            WeatherType.Storm => 25f,
            WeatherType.Fog => 3f,
            WeatherType.Snow => 8f,
            _ => 2f
        };
        
        _state.WindSpeed = MathF.Lerp(_state.WindSpeed, baseWindSpeed, deltaTime * 0.5f);
        _state.Weather.WindSpeed = _state.WindSpeed;
        _state.Weather.WindDirection = _state.WindDirection;
    }

    private void ChangeWeather()
    {
        var roll = _random.NextDouble();
        
        var weatherWeights = _currentSeason switch
        {
            Season.Winter => (0.3, 0.2, 0.15, 0.1, 0.1, 0.15),
            Season.Summer => (0.5, 0.2, 0.15, 0.05, 0.05, 0.05),
            Season.Spring => (0.4, 0.25, 0.15, 0.1, 0.05, 0.05),
            Season.Autumn => (0.35, 0.25, 0.15, 0.1, 0.1, 0.05),
            _ => (0.4, 0.2, 0.15, 0.1, 0.1, 0.05)
        };

        double cumulative = 0;
        
        if (roll < (cumulative += weatherWeights.Item1))
            _currentWeather = WeatherType.Clear;
        else if (roll < (cumulative += weatherWeights.Item2))
            _currentWeather = WeatherType.Cloudy;
        else if (roll < (cumulative += weatherWeights.Item3))
            _currentWeather = WeatherType.Rain;
        else if (roll < (cumulative += weatherWeights.Item4))
            _currentWeather = WeatherType.Storm;
        else if (roll < (cumulative += weatherWeights.Item5))
            _currentWeather = WeatherType.Fog;
        else
            _currentWeather = WeatherType.Snow;

        ApplyWeatherTargets();
        
        Console.WriteLine($"[EnvironmentSystem] Weather: {_currentWeather}");
    }

    private void ApplyWeatherTargets()
    {
        switch (_currentWeather)
        {
            case WeatherType.Clear:
                _targetFogDensity = 0.001f;
                _targetWeatherIntensity = 0f;
                _targetVisibility = 1.0f;
                break;
            case WeatherType.Cloudy:
                _targetFogDensity = 0.004f;
                _targetWeatherIntensity = 0.3f;
                _targetVisibility = 0.9f;
                break;
            case WeatherType.Rain:
                _targetFogDensity = 0.008f;
                _targetWeatherIntensity = 0.6f;
                _targetVisibility = 0.7f;
                break;
            case WeatherType.Storm:
                _targetFogDensity = 0.018f;
                _targetWeatherIntensity = 0.9f;
                _targetVisibility = 0.4f;
                break;
            case WeatherType.Fog:
                _targetFogDensity = 0.03f;
                _targetWeatherIntensity = 0.4f;
                _targetVisibility = 0.3f;
                break;
            case WeatherType.Snow:
                _targetFogDensity = 0.012f;
                _targetWeatherIntensity = 0.5f;
                _targetVisibility = 0.6f;
                break;
        }
    }

    private void ChangeSeason()
    {
        _currentSeason = _currentSeason switch
        {
            Season.Spring => Season.Summer,
            Season.Summer => Season.Autumn,
            Season.Autumn => Season.Winter,
            Season.Winter => Season.Spring,
            _ => Season.Summer
        };
        
        _state.Season = _currentSeason.ToString();
        Console.WriteLine($"[EnvironmentSystem] Season changed to: {_currentSeason}");
    }

    private void UpdateEnvironment()
    {
        UpdateSunPosition();
        UpdateLighting();
    }

    private string LerpColorHex(string color1, string color2, float t)
    {
        var c1 = ParseHexColor(color1);
        var c2 = ParseHexColor(color2);

        var r = (int)MathF.Lerp(c1.r, c2.r, t);
        var g = (int)MathF.Lerp(c1.g, c2.g, t);
        var b = (int)MathF.Lerp(c1.b, c2.b, t);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private (int r, int g, int b) ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        return (
            Convert.ToInt32(hex.Substring(0, 2), 16),
            Convert.ToInt32(hex.Substring(2, 2), 16),
            Convert.ToInt32(hex.Substring(4, 2), 16)
        );
    }

    public void SetTimeOfDay(float hour)
    {
        _state.TimeOfDay = Math.Clamp(hour, 0f, 24f);
        UpdateEnvironment();
    }

    public void SetWeather(string weatherType)
    {
        if (Enum.TryParse<WeatherType>(weatherType, true, out var weather))
        {
            _currentWeather = weather;
            ApplyWeatherTargets();
        }
    }

    public void SetSeason(string season)
    {
        if (Enum.TryParse<Season>(season, true, out var s))
        {
            _currentSeason = s;
            _state.Season = _currentSeason.ToString();
        }
    }

    public void SetTimeScale(float scale)
    {
        _state.TimeScale = Math.Clamp(scale, 0.1f, 10f);
    }

    public void Shutdown()
    {
        Console.WriteLine($"[EnvironmentSystem] Shutdown - Total days simulated: {_dayCount}");
    }
}
