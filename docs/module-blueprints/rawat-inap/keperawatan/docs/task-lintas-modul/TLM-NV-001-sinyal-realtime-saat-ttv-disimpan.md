# TLM-NV-001 — Sinyal Realtime Saat TTV Disimpan, untuk Display Nurse View

```yaml
dokumen_id: TLM-NV-001
jenis: permintaan perubahan lintas modul
modul_peminta: nurse-view
pemilik_peminta: Rizki Gunawan
modul_tujuan: rawat-inap
submodul_tujuan: keperawatan
pemilik_tujuan: Muhammad Hamzah
tanggal: "2026-10-28"
status: DISETUJUI — Muhammad Hamzah, 2026-10-28 (disampaikan Rizki Gunawan); belum ada kode yang diubah
keputusan_asal: [NV-DEC-023, NV-DEC-026]
open_question_asal: NV-OQ-016
source_sha_backend: "a6f977a3 (rizkiG)"
source_sha_frontend: "76b0c6e4b (RizkiV2)"
rujukan:
  - ../../../../nurse-view/00-interview-decisions.md
  - ../../../../nurse-view/01-existing-capability-map.md
```

---

## 1. Ringkasan

Modul **Nurse View** milik Rizki Gunawan adalah TV pasif di nurse station, satu per lantai, yang
**hanya membaca** data pasien rawat inap per bed: status bed, perawat PJ, TTV/EWS terakhir, dan
ikon keselamatan. Nurse View tidak menulis data keperawatan apa pun.

Agar pasien dengan EWS kritis terlihat di TV dalam hitungan detik, Nurse View meminta **satu**
perubahan kecil di kode TTV: setelah TTV pasien rawat inap tersimpan, sistem mengirim **sinyal
"kartu bed ini perlu dimuat ulang"** ke TV lantai pasien itu. Sinyal **tidak membawa data pasien
maupun nilai TTV**; TV mengambil data samaran lewat API Nurse View sendiri.

Tanpa perubahan ini Nurse View tetap berjalan: TV memuat ulang penuh setiap 60 detik. Jadi
permintaan ini **menyegerakan**, bukan **memungkinkan**.

## 2. Mengapa menyentuh keperawatan

TTV rawat inap dicatat perawat lewat form pengawasan harian, yang menyimpan ke
`TrxPatientVitalSign` melalui `PatientVitalSignController` (Clinical Management). Di sanalah EWS
dihitung (`PatientVitalSignCalculation`). Satu-satunya tempat yang tahu "TTV baru saja berubah"
adalah titik simpan itu.

| Bukti | Lokasi |
|---|---|
| Simpan TTV baru | `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` — `[HttpPost]` baris 452 |
| Ubah TTV | baris 603 — `[HttpPut("{id:guid}")]` |
| Batalkan TTV | baris 820 — `[HttpPatch("{id:guid}/cancel")]` |
| Episode diisi server | `Areas/HealthServices/ClinicalManagement/Services/InpatientVitalSignService.cs` |
| EWS dihitung saat simpan | `PatientVitalSignController.cs` baris 559–564 |

## 3. Perubahan yang diminta

| No | Perubahan | Keterangan |
|---:|---|---|
| 1 | Sesudah simpan **berhasil di-commit** pada `POST`, `PUT`, dan `PATCH cancel`, panggil satu metode milik Nurse View, misalnya `NvwRealtimeService.NotifyVitalSignChangedAsync(inpEpisodeId)` | Satu baris per aksi. Hanya bila `InpEpisodeId` terisi; TTV poliklinik dan IGD tidak terpengaruh |
| 2 | Pemanggilan bersifat **best-effort** | Bila hub gagal, simpan TTV **tetap berhasil** dan galatnya hanya dicatat di log. Polanya sama dengan `InpBedRealtimeService.NotifyBedAcquiredAsync` |

`PATCH verify` dan `PATCH notify-doctor` **tidak** perlu memanggilnya karena nilai TTV dan EWS
tidak berubah.

**Yang dikirim ke TV — contoh lengkap:**

```json
{
  "eventType": "VitalSignChanged",
  "bedId": "…",
  "floorCode": "3",
  "occurredAtUtc": "2026-10-28T07:20:00Z"
}
```

Tidak ada `patientName`, `medicalRecordNumber`, nilai TTV, maupun EWS. TV yang menerima sinyal
itu memanggil API Nurse View untuk memuat ulang satu kartu.

**Contoh alur.** Pukul 14.20 perawat Sari menyimpan TTV pasien di bed 305-A dengan EWS 8
(`Critical`):

1. `PatientVitalSignController` menyimpan baris TTV seperti hari ini.
2. Sesudah commit, metode Nurse View dipanggil dengan episode pasien itu.
3. Nurse View mencari bed dan lantai pasien, lalu mengirim sinyal ke grup TV lantai 3.
4. TV lantai 3 memuat ulang kartu 305-A, yang dalam beberapa detik berubah merah.
5. Bila langkah 2–3 gagal, TTV tetap tersimpan; TV melihatnya paling lambat 60 detik kemudian.

### 3.1 Permintaan 2 — pemetaan profesi yang dapat dipakai ulang

*Ditambahkan 2026-10-28 dari `design-business-module` Nurse View.*

Header papan Nurse View menampilkan perawat jaga dari roster HR, dan hanya profesi perawat atau
bidan yang boleh muncul. Aturan pengenalan profesi itu sudah ada di keperawatan:
`InpatientClinicalContextService.MapCpptProfessionType`. Metode itu berstatus `private static`.

| Perubahan | Keterangan |
|---|---|
| `private static` → `internal static` pada `MapCpptProfessionType` | Tanpa perubahan isi maupun perilaku. Nurse View memanggilnya agar aturan "siapa perawat" tetap satu, bukan disalin |

**Contoh.** Profesi pegawai bernama "Perawat Pelaksana" dipetakan `Nurse`, sehingga tampil di
header. Profesi "Petugas Administrasi Bangsal" dipetakan `Other`, sehingga tidak tampil.

Bila Hamzah menolak, header Nurse View menunggu, dan aturan profesi **tidak** disalin ke modul
Nurse View.

## 4. Dampak bagi keperawatan

| Aspek | Dampak |
|---|---|
| Perilaku simpan/ubah/batal TTV | Tidak berubah |
| Request, response, kode status endpoint | Tidak berubah |
| Validasi, hak akses `PatientVitalSign` | Tidak berubah |
| Tabel, kolom, migration | **Tidak ada** |
| Waktu respons simpan | Bertambah sangat kecil; pengiriman best-effort dan tidak menahan transaksi |
| Layar keperawatan frontend | Tidak berubah |

## 5. Yang dibutuhkan dari Muhammad Hamzah

| No | Pertanyaan | Pilihan |
|---:|---|---|
| 1 | Setuju satu titik panggil ini ditambahkan? | Setuju / tolak / setuju dengan syarat |
| 2 | Siapa yang menuliskan perubahannya di `PatientVitalSignController`? | Rizki dalam task Nurse View, dengan laporan task disalin ke folder ini / Hamzah sendiri |
| 3 | Konfirmasi pemilik `PatientVitalSignController` | Bila bukan Hamzah, mohon sebut pemiliknya agar permintaan diteruskan |
| 4 | Setuju `MapCpptProfessionType` dijadikan `internal static` (permintaan 2)? | Setuju / tolak |

## 6. Kriteria terima

| ID | Kriteria |
|---|---|
| `TLM-NV-001-AC-1` | **Given** TTV pasien rawat inap disimpan **When** commit berhasil **Then** TV lantai pasien menerima satu sinyal `VitalSignChanged` |
| `TLM-NV-001-AC-2` | **Given** hub SignalR tidak tersedia **When** perawat menyimpan TTV **Then** simpan tetap `200`/`201` dan TTV tersimpan |
| `TLM-NV-001-AC-3` | **Given** TTV pasien poliklinik atau IGD (tanpa `InpEpisodeId`) **When** disimpan **Then** tidak ada sinyal terkirim |
| `TLM-NV-001-AC-4` | **Given** sinyal terkirim **Then** isinya tidak memuat nama, No. RM, nilai TTV, maupun EWS |

## 7. Status dan tindak lanjut

| Tanggal | Status | Catatan |
|---|---|---|
| 2026-10-28 | `USULAN` | Ditulis dari closure pass Nurse View. Desain Nurse View (`design-business-module`) belum dijalankan; nama metode di bagian 3 masih contoh dan dikunci di kontrak Nurse View |
| 2026-10-28 | `USULAN` | Desain Nurse View `draft` selesai: nama metode dikunci `NvwRealtimeService.NotifyVitalSignChangedAsync`, event `NurseViewBoardChanged`. Ditambah permintaan 2 (bagian 3.1) |
| 2026-10-28 | `DISETUJUI` | Muhammad Hamzah menyetujui permintaan 1 (titik terbit TTV) dan permintaan 2 (`MapCpptProfessionType` → `internal static`), disampaikan Rizki Gunawan. **Masih terbuka:** pertanyaan 2 dan 3 bagian 5 (siapa yang menulis kodenya; konfirmasi pemilik `PatientVitalSignController`) dijawab saat task dibuat. Laporan task implementasinya disalin/dirujuk ke folder ini |

Dokumen ini diperbarui bila jawaban Hamzah masuk atau bila implementasinya berjalan.
