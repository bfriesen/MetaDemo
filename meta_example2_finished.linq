<Query Kind="Program">
  <NuGetReference>Reflectinator</NuGetReference>
  <Namespace>Reflectinator</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#define NONEST
void Main() // Assembly Scanning
{
    // Note that the binary format of the packets defines the packet number
    // as the first two bytes of the byte array as a 16-bit integer .

//    Stream data = GetSampleGammaPacketData();
//    Stream data = GetSampleGammaSpectrumPacketData();
//    Stream data = GetSampleNeutronPacketData();
    Stream data = GetSampleGpsPacketData();
 
    PacketFactory packetFactory = new PacketFactory();
       
    Packet packet = packetFactory.GetPacket(data);
    packet.Dump();
}

public class PacketFactory
{
    private readonly Dictionary<short, Type> _packetFactoryMap = new Dictionary<short, Type>();

    public PacketFactory()
    {
        var packetTypes = typeof(Packet).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(Packet).IsAssignableFrom(t));
        
        foreach (var packetType in packetTypes)
        {
            var packet = (Packet)Activator.CreateInstance(packetType);
            var packetNumber = packet.PacketNumber;
            _packetFactoryMap.Add(packetNumber, packetType);
        }
    }

    public Packet GetPacket(Stream stream)
    {
        Packet packet;
            
        using (var reader = new BinaryReader(stream))
        {
            var packetNumber = reader.ReadInt16();
            
            var packetType = _packetFactoryMap[packetNumber];
            packet = (Packet)Activator.CreateInstance(packetType);
            
            packet.Load(reader);
        }
        
        return packet;
    }
}

public abstract class Packet
{
    public byte TickCount { get; set; }
    public abstract short PacketNumber { get; }
    
    public abstract void Load(BinaryReader reader);
}

public class GammaPacket : Packet
{
    public int Count { get; set; }
    public override short PacketNumber { get { return 1; } }
    
    public override void Load(BinaryReader reader)
    {
        TickCount = reader.ReadByte();
        Count = reader.ReadInt32();
    }
}

public class GammaSpectrumPacket : GammaPacket
{
    public short[] Spectrum { get; set; }
    public override short PacketNumber { get { return 2; } }
    
    public override void Load(BinaryReader reader)
    {
        base.Load(reader);
        
        Spectrum = new short[1024];
        
        for (int i = 0; i < 1024; i++)
        {
            Spectrum[i] = reader.ReadInt16();
        }
    }
}

public class NeutronPacket : Packet
{
    public int Count { get; private set; }
    public override short PacketNumber { get { return 3; } }
    
    public override void Load(BinaryReader reader)
    {
        TickCount = reader.ReadByte();
        Count = reader.ReadInt32();
    }
}

public class GpsPacket : Packet
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public override short PacketNumber { get { return 4; } }

    public override void Load(BinaryReader reader)
    {
        TickCount = reader.ReadByte();
        Latitude = reader.ReadDouble();
        Longitude = reader.ReadDouble();
    }
}

private Stream GetSampleGammaPacketData()
{
    var memoryStream = new MemoryStream();
    var writer = new BinaryWriter(memoryStream);
    
    writer.Write((short)1);
    writer.Write((byte)19);
    writer.Write(1294);
    
    writer.Flush();
    memoryStream.Seek(0, SeekOrigin.Begin);
    
    return memoryStream;
}

private Stream GetSampleGammaSpectrumPacketData()
{
    var memoryStream = new MemoryStream();
    var writer = new BinaryWriter(memoryStream);
    
    writer.Write((short)2);
    writer.Write((byte)19);
    writer.Write(1294);
    
    Random random = new Random();
    for (int i = 0; i < 1024; i++)
    {
        var multiplier = (1024 - i) / 8;
        writer.Write((short)random.Next(5 * multiplier, 20 * multiplier)); 
    }
    
    writer.Flush();
    memoryStream.Seek(0, SeekOrigin.Begin);
    
    return memoryStream;
}

private Stream GetSampleNeutronPacketData()
{
    var memoryStream = new MemoryStream();
    var writer = new BinaryWriter(memoryStream);
    
    writer.Write((short)3);
    writer.Write((byte)19);
    writer.Write(14);
    
    writer.Flush();
    memoryStream.Seek(0, SeekOrigin.Begin);
    
    return memoryStream;
}

#region Super Secret

private Stream GetSampleGpsPacketData()
{
    var memoryStream = new MemoryStream();
    var writer = new BinaryWriter(memoryStream);
    
    writer.Write((short)4);
    writer.Write((byte)19);
    writer.Write(42.49);
    writer.Write(-83.38);
    
    writer.Flush();
    memoryStream.Seek(0, SeekOrigin.Begin);
    
    return memoryStream;
}

#endregion