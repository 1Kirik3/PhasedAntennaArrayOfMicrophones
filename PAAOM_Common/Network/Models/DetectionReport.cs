using System;
using System.Buffers.Binary;

namespace PAAOM_Common.Network.Models
{
    public class DetectionReport : PacketBase
    {
        public override byte Type => 0x04;

        public uint DetectionTimeUnix { get; set; }       // 4 байта
        public ushort DetectionTimeFine { get; set; }    // 2 байта (десятитысячные доли секунды?)
        public ushort MeasurementNumber { get; set; }    // 2 байта
        public ushort TargetType { get; set; }            // 2 байта
        public ushort Snr { get; set; }                  // 2 байта (ОСП)
        public float Bearing { get; set; }               // 4 байта
        public float BearingRate { get; set; }           // 4 байта (ВИП)
        public ushort Distance { get; set; }             // 2 байта
        public float DistanceRate { get; set; }          // 4 байта (ВИФ)
        public ushort AngleStdDev { get; set; }          // 2 байта (СКО угла, в десятых долях)
        public ushort TimeStdDev { get; set; }           // 2 байта (СКО времени, в см)

        public override ushort CalculateBodyLength() => 30; // 4+2+2+2+2+4+4+2+4+2+2

        public override void WriteBody(Span<byte> buffer)
        {
            int offset = 0;
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset, 4), DetectionTimeUnix);
            offset += 4;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), DetectionTimeFine);
            offset += 2;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), MeasurementNumber);
            offset += 2;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), TargetType);
            offset += 2;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), Snr);
            offset += 2;

            byte[] bearingBytes = BitConverter.GetBytes(Bearing);
            bearingBytes.CopyTo(buffer.Slice(offset, 4));
            offset += 4;

            byte[] bearingRateBytes = BitConverter.GetBytes(BearingRate);
            bearingRateBytes.CopyTo(buffer.Slice(offset, 4));
            offset += 4;

            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), Distance);
            offset += 2;

            byte[] distanceRateBytes = BitConverter.GetBytes(DistanceRate);
            distanceRateBytes.CopyTo(buffer.Slice(offset, 4));
            offset += 4;

            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), AngleStdDev);
            offset += 2;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset, 2), TimeStdDev);
        }

        public static DetectionReport ReadBody(ReadOnlySpan<byte> bodyBuffer)
        {
            int offset = 0;
            var report = new DetectionReport();

            report.DetectionTimeUnix = BinaryPrimitives.ReadUInt32LittleEndian(bodyBuffer.Slice(offset, 4));
            offset += 4;
            report.DetectionTimeFine = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;
            report.MeasurementNumber = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;
            report.TargetType = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;
            report.Snr = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;

            report.Bearing = BitConverter.ToSingle(bodyBuffer.Slice(offset, 4).ToArray(), 0);
            offset += 4;

            report.BearingRate = BitConverter.ToSingle(bodyBuffer.Slice(offset, 4).ToArray(), 0);
            offset += 4;

            report.Distance = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;

            report.DistanceRate = BitConverter.ToSingle(bodyBuffer.Slice(offset, 4).ToArray(), 0);
            offset += 4;

            report.AngleStdDev = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));
            offset += 2;
            report.TimeStdDev = BinaryPrimitives.ReadUInt16LittleEndian(bodyBuffer.Slice(offset, 2));

            return report;
        }
    }
}
