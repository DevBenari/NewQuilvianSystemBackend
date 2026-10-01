using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Perhitungan turunan tanda vital — BMI, MAP, GCS, EWS, dan penanda abnormal/kritis.
    /// </summary>
    /// <remarks>
    /// <c>BE-RWI-141</c>. Dipindahkan apa adanya dari <c>PatientVitalSignController</c>, tanpa satu
    /// ambang pun berubah, supaya tanda vital yang diukur dokter pada SOAP rawat inap dinilai dengan
    /// aturan yang sama persis dengan ukuran perawat. Contoh: RR 26 x/m, SpO2 93%, suhu 38,6 °C,
    /// sistolik 100, nadi 124 menghasilkan EWS 3+2+1+2+2 = 10 (Critical) di kedua jalur.
    /// </remarks>
    public static class PatientVitalSignCalculation
    {
        /// <summary>Menghitung BMI, MAP, GCS, EWS, serta tanda abnormal/kritis dari nilai terukur.</summary>
        public static CalculatedVitalSignValue Calculate(
            decimal? weight,
            decimal? height,
            int? systolic,
            int? diastolic,
            int? respiratoryRate,
            decimal? oxygenSaturation,
            decimal? temperature,
            int? pulseRate,
            ConsciousnessStatus consciousnessStatus,
            int? gcsEye,
            int? gcsVerbal,
            int? gcsMotor)
        {
            var bmi = CalculateBmi(weight, height);
            var map = CalculateMap(systolic, diastolic);
            var mapStatus = CalculateMapStatus(map);
            var gcsTotal = CalculateGcsTotal(gcsEye, gcsVerbal, gcsMotor);
            var ewsScore = CalculateEwsScore(respiratoryRate, oxygenSaturation, temperature, systolic, pulseRate, consciousnessStatus);
            var ewsRiskLevel = CalculateEwsRiskLevel(ewsScore);
            var ewsMonitoringRecommendation = GetEwsMonitoringRecommendation(ewsRiskLevel, ewsScore);
            var isCritical = CalculateIsCritical(systolic, diastolic, pulseRate, respiratoryRate, temperature, oxygenSaturation, gcsTotal, ewsRiskLevel);
            var isAbnormal = isCritical || CalculateIsAbnormal(systolic, diastolic, pulseRate, respiratoryRate, temperature, oxygenSaturation, gcsTotal, mapStatus, ewsRiskLevel);

            return new CalculatedVitalSignValue
            {
                BMI = bmi,
                MeanArterialPressure = map,
                MapStatus = mapStatus,
                GcsTotal = gcsTotal,
                EarlyWarningScore = ewsScore,
                EwsRiskLevel = ewsRiskLevel,
                EwsMonitoringRecommendation = ewsMonitoringRecommendation,
                IsAbnormal = isAbnormal,
                IsCritical = isCritical
            };
        }

        private static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
        {
            if (!weightKg.HasValue || !heightCm.HasValue || heightCm.Value <= 0)
                return null;

            var heightMeter = heightCm.Value / 100;
            var bmi = weightKg.Value / (heightMeter * heightMeter);

            return Math.Round(bmi, 2);
        }

        private static decimal? CalculateMap(int? systolic, int? diastolic)
        {
            if (!systolic.HasValue || !diastolic.HasValue)
                return null;

            var map = diastolic.Value + ((systolic.Value - diastolic.Value) / 3m);

            return Math.Round(map, 2);
        }

        private static MapStatus CalculateMapStatus(decimal? map)
        {
            if (!map.HasValue)
                return MapStatus.Unknown;

            if (map.Value < 60)
                return MapStatus.Hypotension;

            if (map.Value > 100)
                return MapStatus.Hypertension;

            return MapStatus.Normal;
        }

        private static int? CalculateGcsTotal(int? eye, int? verbal, int? motor)
        {
            if (!eye.HasValue && !verbal.HasValue && !motor.HasValue)
                return null;

            if (!eye.HasValue || !verbal.HasValue || !motor.HasValue)
                return null;

            return eye.Value + verbal.Value + motor.Value;
        }

        private static int? CalculateEwsScore(
            int? respiratoryRate,
            decimal? oxygenSaturation,
            decimal? temperature,
            int? systolicBloodPressure,
            int? pulseRate,
            ConsciousnessStatus consciousnessStatus)
        {
            var hasAnyValue =
                respiratoryRate.HasValue ||
                oxygenSaturation.HasValue ||
                temperature.HasValue ||
                systolicBloodPressure.HasValue ||
                pulseRate.HasValue ||
                consciousnessStatus != ConsciousnessStatus.Unknown;

            if (!hasAnyValue)
                return null;

            var score = 0;

            if (respiratoryRate.HasValue)
            {
                if (respiratoryRate.Value <= 8) score += 3;
                else if (respiratoryRate.Value <= 11) score += 1;
                else if (respiratoryRate.Value <= 20) score += 0;
                else if (respiratoryRate.Value <= 24) score += 2;
                else score += 3;
            }

            if (oxygenSaturation.HasValue)
            {
                if (oxygenSaturation.Value <= 91) score += 3;
                else if (oxygenSaturation.Value <= 93) score += 2;
                else if (oxygenSaturation.Value <= 95) score += 1;
                else score += 0;
            }

            if (temperature.HasValue)
            {
                if (temperature.Value <= 35.0m) score += 3;
                else if (temperature.Value <= 36.0m) score += 1;
                else if (temperature.Value <= 38.0m) score += 0;
                else if (temperature.Value <= 39.0m) score += 1;
                else score += 2;
            }

            if (systolicBloodPressure.HasValue)
            {
                if (systolicBloodPressure.Value <= 90) score += 3;
                else if (systolicBloodPressure.Value <= 100) score += 2;
                else if (systolicBloodPressure.Value <= 110) score += 1;
                else if (systolicBloodPressure.Value <= 219) score += 0;
                else score += 3;
            }

            if (pulseRate.HasValue)
            {
                if (pulseRate.Value <= 40) score += 3;
                else if (pulseRate.Value <= 50) score += 1;
                else if (pulseRate.Value <= 90) score += 0;
                else if (pulseRate.Value <= 110) score += 1;
                else if (pulseRate.Value <= 130) score += 2;
                else score += 3;
            }

            if (consciousnessStatus != ConsciousnessStatus.Unknown &&
                consciousnessStatus != ConsciousnessStatus.ComposMentis)
            {
                score += 3;
            }

            return score;
        }

        private static EwsRiskLevel CalculateEwsRiskLevel(int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return EwsRiskLevel.Unknown;

            if (ewsScore.Value >= 7)
                return EwsRiskLevel.Critical;

            if (ewsScore.Value >= 5)
                return EwsRiskLevel.High;

            if (ewsScore.Value >= 3)
                return EwsRiskLevel.Medium;

            return EwsRiskLevel.Low;
        }

        private static string? GetEwsMonitoringRecommendation(EwsRiskLevel riskLevel, int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return null;

            return riskLevel switch
            {
                EwsRiskLevel.Low => "Monitoring rutin sesuai kondisi klinis pasien.",
                EwsRiskLevel.Medium => "Monitoring ulang tanda vital dan evaluasi klinis berkala.",
                EwsRiskLevel.High => "Monitoring lebih sering dan informasikan dokter penanggung jawab.",
                EwsRiskLevel.Critical => "Pemantauan terus menerus tanda-tanda vital, pertimbangkan eskalasi klinis segera.",
                _ => null
            };
        }

        private static bool CalculateIsCritical(
            int? systolic,
            int? diastolic,
            int? pulseRate,
            int? respiratoryRate,
            decimal? temperature,
            decimal? oxygenSaturation,
            int? gcsTotal,
            EwsRiskLevel ewsRiskLevel)
        {
            return
                ewsRiskLevel == EwsRiskLevel.Critical ||
                (systolic.HasValue && (systolic.Value <= 90 || systolic.Value >= 220)) ||
                (diastolic.HasValue && diastolic.Value >= 120) ||
                (pulseRate.HasValue && (pulseRate.Value <= 40 || pulseRate.Value >= 131)) ||
                (respiratoryRate.HasValue && (respiratoryRate.Value <= 8 || respiratoryRate.Value >= 25)) ||
                (temperature.HasValue && (temperature.Value <= 35 || temperature.Value >= 40)) ||
                (oxygenSaturation.HasValue && oxygenSaturation.Value <= 91) ||
                (gcsTotal.HasValue && gcsTotal.Value <= 8);
        }

        private static bool CalculateIsAbnormal(
            int? systolic,
            int? diastolic,
            int? pulseRate,
            int? respiratoryRate,
            decimal? temperature,
            decimal? oxygenSaturation,
            int? gcsTotal,
            MapStatus mapStatus,
            EwsRiskLevel ewsRiskLevel)
        {
            return
                ewsRiskLevel == EwsRiskLevel.Medium ||
                ewsRiskLevel == EwsRiskLevel.High ||
                mapStatus == MapStatus.Hypotension ||
                mapStatus == MapStatus.Hypertension ||
                (systolic.HasValue && (systolic.Value < 100 || systolic.Value > 180)) ||
                (diastolic.HasValue && (diastolic.Value < 60 || diastolic.Value > 110)) ||
                (pulseRate.HasValue && (pulseRate.Value < 50 || pulseRate.Value > 110)) ||
                (respiratoryRate.HasValue && (respiratoryRate.Value < 12 || respiratoryRate.Value > 24)) ||
                (temperature.HasValue && (temperature.Value < 36 || temperature.Value > 38)) ||
                (oxygenSaturation.HasValue && oxygenSaturation.Value < 95) ||
                (gcsTotal.HasValue && gcsTotal.Value < 15);
        }

        /// <summary>Merapikan isian yang saling bergantung sebelum baris tanda vital disimpan.</summary>
        public static void Normalize(TrxPatientVitalSign entity)
        {
            if (!entity.IsActive ||
                entity.VitalSignStatus == PatientVitalSignStatus.Cancelled ||
                entity.VitalSignStatus == PatientVitalSignStatus.EnteredInError)
            {
                entity.NeedDoctorNotification = false;
            }

            if (!entity.HasPain)
            {
                entity.PainScale = null;
                entity.PainLocation = null;
                entity.PainNote = null;
            }

            if (!entity.IsUsingOxygen)
            {
                entity.OxygenSupportType = OxygenSupportType.None;
                entity.OxygenFlowRate = null;
                entity.OxygenSupportNote = null;
            }
        }
    }

    public sealed class CalculatedVitalSignValue
    {
        public decimal? BMI { get; set; }
        public decimal? MeanArterialPressure { get; set; }
        public MapStatus MapStatus { get; set; }
        public int? GcsTotal { get; set; }
        public int? EarlyWarningScore { get; set; }
        public EwsRiskLevel EwsRiskLevel { get; set; }
        public string? EwsMonitoringRecommendation { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsCritical { get; set; }
    }
}
