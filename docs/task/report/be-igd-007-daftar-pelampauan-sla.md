# Laporan Perubahan Backend — `BE-IGD-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-007` |
| Judul | Perawat dapat mengambil daftar pasien yang melampaui batas |
| Slice | S2 — Pasien menunggu terlalu lama tertandai |
| Trace | api contract `GET /emergency-triages/sla-breaches`; permission matrix `EmergencyTriage : Read`; `AT-IGD-024` |
| Dependency | `BE-IGD-006` — sudah dikerjakan |
| Commit backend | `21c609f2853574532f74dd2b1489b8d2e502abd1` |
| Tanggal | 18 Agustus 2026 |
| **Status** | **Belum selesai — belum pernah dikompilasi** |

---

## 1. Masalah yang diperbaiki

Penanda breach yang diisi `BE-IGD-006` tersimpan pada masing-masing penilaian. Tanpa endpoint
ini, kepala jaga harus membuka kunjungan satu per satu untuk tahu siapa yang terlambat.

## 2. Proses bisnis

### 2.1 Pelaku

Kepala jaga dan perawat yang memegang hak `EmergencyTriage : Read`.

### 2.2 Aturan yang ditegakkan

| No | Aturan | Cara ditegakkan |
| --- | --- | --- |
| 1 | Hanya yang benar-benar melampaui batas | Saringan `IsSlaBreached` bernilai benar |
| 2 | Pasien yang sudah ditangani tidak muncul, penandanya tetap tersimpan | Saringan `TreatmentStartedAt == null` saat query, bukan penghapusan penanda |
| 3 | Penyaringan unit dan rentang waktu | Query `serviceUnitId`, `startDate`, `endDate` |
| 4 | Tanpa hak akses ditolak 403 | `[AccessPermission("EmergencyTriage", "Read")]` |
| 5 | Memuat nama pasien, bukan hanya identifier | `PatientName`, dengan cadangan alias |

Aturan kedua adalah inti desainnya: penanda breach bersifat permanen sebagai riwayat mutu,
tetapi daftar kerja hanya menampilkan yang masih perlu ditindaklanjuti. Keduanya dipisahkan
supaya laporan mutu tidak kehilangan kejadian masa lalu.

### 2.3 Rentang waktu disaring pada kejadian, bukan pada penilaian

`startDate` dan `endDate` disaring terhadap `SlaBreachedAt`, bukan `StartedAt`, karena yang
dicari pemakai daftar ini adalah kejadian keterlambatannya. Ini berbeda dari `GET /` biasa yang
menyaring pada `StartedAt`, dan perbedaan itu disengaja.

## 3. File yang diubah

| File | Perubahan |
| --- | --- |
| `DTOs/EmergencyTriageDtos.cs` | `EmergencyTriageSlaBreachResponse` |
| `Services/EmergencyTriageService.cs` | `GetSlaBreachesAsync` |
| `Controller/EmergencyTriageController.cs` | `GET /sla-breaches` |

Route `sla-breaches` berdampingan dengan `{id:guid}`. Keduanya tidak pernah saling menangkap
karena `sla-breaches` bukan Guid yang sah.

## 4. Kolom sensitif yang sengaja dikecualikan

Kedelapan kolom bertanda sensitif pada data dictionary **tidak** ikut dalam balasan:
`TriageReason`, `AirwaySummary`, `BreathingSummary`, `CirculationSummary`, `DisabilitySummary`,
`ExposureSummary`, `RedFlagSummary`, dan `Notes`.

Daftar ini dipakai untuk menentukan siapa yang harus didahulukan, bukan untuk membaca isi
penilaiannya. Yang ikut hanyalah identitas secukupnya, level triase, waktu, dan lama
keterlambatan.

`MedicalRecordNumber` ikut disertakan. Kolom itu tidak bertanda sensitif pada data dictionary
dan diperlukan petugas untuk menemukan pasiennya. Bila security/privacy owner menilai
sebaliknya, mengeluarkannya cukup menghapus satu properti pada DTO.

## 5. Pasien tak dikenal — kasus yang tidak disebut roadmap

`TrxEmergencyVisit.PatientId` boleh kosong dan modul ini memang melayani pasien yang belum
teridentifikasi. `PatientName` karena itu diisi berjenjang: nama pasien bila ada, lalu
`TemporaryPatientAlias`, lalu teks "Pasien belum teridentifikasi".

Tanpa penanganan ini, kriteria "balasan memuat nama pasien" akan bocor menjadi nama kosong
persis pada pasien yang paling gawat, yaitu pasien tak dikenal yang dibawa dalam keadaan
darurat.

## 6. Verifikasi

**Belum ada verifikasi berjalan.** Build tidak dijalankan; `AT-IGD-024` tidak punya tempat
untuk ditulis karena solution tidak memiliki test project.

| Kriteria | Status |
| --- | --- |
| 1. Hanya yang melampaui dan belum ditangani | Ada di kode — **belum terbukti** |
| 2. Pasien tertangani hilang dari daftar, penanda tetap | Ada di kode — **belum terbukti** |
| 3. Penyaringan unit dan rentang waktu | Ada di kode — **belum terbukti** |
| 4. Tanpa hak akses ditolak 403 | Ada di kode — **belum terbukti** |
| 5. Balasan memuat nama pasien | Ada di kode — **belum terbukti** |

## 7. Risiko tersisa

| No | Risiko | Penanganan |
| ---: | --- | --- |
| 1 | Daftar menampilkan data pasien | Kolom sensitif dikecualikan; perlu tinjauan security/privacy owner |
| 2 | Kolom breach belum ada di database | Endpoint gagal saat runtime sampai migration `BE-IGD-005` diterapkan |
| 3 | `MedicalRecordNumber` ikut dalam balasan | Menunggu penegasan security/privacy owner |
