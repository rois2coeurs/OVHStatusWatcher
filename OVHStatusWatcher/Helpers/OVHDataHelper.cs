namespace OVHStatusWatcher.Helpers;

public static class OvhDataHelper
{
    public static bool IsRegion(string value)
    {
        return value.Length == 3;
    }

    public static bool IsRack(string value)
    {
        return value.Contains("Rack", StringComparison.OrdinalIgnoreCase);
    }

    public static string ExtractRackNumber(string value)
    {
        var values = value.Split(" ");
        int? rackStringIndex = null;
        var index = 0;
        foreach (var v in values)
        {
            if (IsRack(v))
            {
                rackStringIndex = index;
                break;
            }

            index++;
        }

        if (rackStringIndex is null) throw new Exception("RackNumber not found");

        return values[1 + (int)rackStringIndex];
    }
    
    public static string[] ExtractEnvs(string value)
    {
        var test = value.Split(']', StringSplitOptions.RemoveEmptyEntries)[0];
        test = test.Remove(test.IndexOf('['), 1);
        test = test.Trim();
        return test.Split('/');
    }

    public static string ExtractRegionFromDatacenter(string value)
    {
        return value[..3];
    }
}