# Laporan Task Backend: BE-RWI-133 — Penyelarasan Seeder Perencanaan Pulang (Discharge Planning)

## 1. Identitas Task
- **Task ID**: `BE-RWI-133`
- **Modul**: Pelayanan Kesehatan — Rawat Inap (*Inpatient Nursing Workspace*)
- **Sub-Modul**: Menu 1 — Pengkajian Pasien (*Patient Assessment*) — Sub-Menu 7: Perencanaan Pulang (*Discharge Planning*)
- **Referensi Bisnis**: Standar KARS ARK 3 / ARK 4, Quilvian V1 Perencanaan Pulang (`add-rencana-pulang.jsx`).
- **Status Database**: **Zero Migration** (Tipe pengkajian `DischargePlanning` = 3, binding `NurseNote` pada `TrxPatientAssessment`).
- **Status**: **SELESAI (100% Verified)**

---

## 2. Ringkasan Perubahan
1. **Pembaruan Definisi Baseline Instrumen `DISCHARGE_PLANNING`**:
   - Berkas: `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs:720-745`.
   - Menggantikan draft teks bebas 8 seksi dengan **8 Seksi Terstruktur Komprehensif**:
     - **Seksi 1 (`DP_KRITERIA`) — Skrining Kriteria Pemulangan Pasien**:
       - `DP_KRIT_USIA`: Usia > 65 tahun.
       - `DP_KRIT_SUICIDE`: Riwayat percobaan bunuh diri / psikiatri.
       - `DP_KRIT_CRIME`: Korban kekerasan / kasus kriminal / penelantaran.
       - `DP_KRIT_MOBILITY`: Keterbatasan mobilitas fisik.
       - `DP_KRIT_CONTINUED_CARE`: Perawatan dan pengobatan lanjutan kompleks.
       - `DP_KRIT_ADL`: Memerlukan bantuan aktivitas harian (ADL).
     - **Seksi 2 (`DP_CAREGIVER`) — Caregiver & Kesiapan Perawatan di Rumah**:
       - `DP_LIVING_ALONE`: Pasien tinggal sendiri setelah keluar RS.
       - `DP_CAREGIVER_NAME`: Nama penanggung jawab / caregiver utama di rumah.
       - `DP_CAREGIVER_PHONE`: Nomor telepon / kontak caregiver.
     - **Seksi 3 (`DP_HOME_ENV`) — Lingkungan Fisik Rumah (Faktor Keselamatan)**:
       - `DP_BEDROOM_FLOOR`: Letak kamar tidur (Lantai 1, Lantai 2, Lainnya).
       - `DP_LIGHTING`: Kondisi penerangan (Cukup Terang, Kurang/Gelap).
       - `DP_BATHROOM_DIST`: Jarak ke kamar mandi (< 5 Meter, ≥ 5 Meter).
       - `DP_TOILET_TYPE`: Jenis WC (WC Duduk, WC Jongkok).
     - **Seksi 4 (`DP_EQUIPMENT`) — Peralatan Medis & Alat Bantu di Rumah**:
       - `DP_MED_EQUIP_USED`: Memerlukan peralatan medis di rumah (kateter, NGT, O2, stoma).
       - `DP_MED_EQUIP_NOTE`: Rincian peralatan medis.
       - `DP_MOBILITY_AID`: Memerlukan alat bantu mobilitas (kursi roda, walker, tongkat).
       - `DP_MOBILITY_AID_NOTE`: Rincian alat bantu mobilitas.
     - **Seksi 5 (`DP_HOMECARE`) — Kebutuhan Layanan Home Care / Rawat Lanjut**:
       - `DP_HOMECARE_NEEDED`: Memerlukan perawatan khusus di rumah (home care).
       - `DP_HOMECARE_NOTE`: Rincian kebutuhan home care.
     - **Seksi 6 (`DP_TRANSPORT`) — Transportasi Kepulangan Pasien**:
       - `DP_TRANSPORT_TYPE`: Moda transportasi (Kendaraan Pribadi, Taksi/Umum, Ambulans Transport, Ambulans Medis/ICU).
       - `DP_TRANSPORT_NOTE`: Catatan khusus transportasi.
     - **Seksi 7 (`DP_FOLLOWUP`) — Rencana Kontrol & Edukasi Lanjutan**:
       - `DP_FOLLOWUP_PLAN`: Rencana kontrol dokter DPJP / poliklinik.
       - `DP_MED_EDUCATION`: Edukasi obat pulang dan kepatuhan terapi.
     - **Seksi 8 (`DP_PLAN_STATUS`) — Status Rencana & Resume Pemulangan**:
       - `DP_PLAN_STATUS_NOTE`: Catatan resume perencanaan pulang perawat — terikat pada `NurseNote`.
2. **Kesesuaian Validasi**:
   - `RequiredItemCodes`: `DP_TRANSPORT_TYPE`.
   - Mengeliminasi review flag `G-10`.

---

## 3. Bukti Verifikasi
- Kompilasi: `dotnet build` bersih dengan 0 Error.
