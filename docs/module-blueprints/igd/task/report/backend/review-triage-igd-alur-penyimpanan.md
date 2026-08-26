# Review Pengerjaan Triage IGD — Alur Penyimpanan

## Metadata

| Field | Nilai |
| --- | --- |
| Jenis | Review dan perbaikan, bukan task roadmap |
| Modul | IGD — pemeriksaan triage |
| Repository | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Tanggal | 20 Agustus 2026 |
| Pemicu | Pertanyaan owner: dokter diisi di menu triage atau di daftar pasien |
| **Status** | **Diperbaiki, belum diuji jalan** |

---

## 1. Ringkasan

Pertanyaan sederhana tentang letak pemilihan dokter membuka temuan yang jauh lebih besar:
**17 dari 20 field pada formulir triage tidak pernah tersimpan.**

Perawat mengisi tanda vital, riwayat penyakit, dan kesimpulan pengkajian, menekan simpan,
lalu melihat pesan berhasil. Yang benar-benar masuk basis data hanya empat field.

Penyebabnya bukan galat yang terlihat. `System.Text.Json` pada ASP.NET Core mengabaikan
properti JSON yang tidak dikenal DTO **tanpa peringatan apa pun**. Permintaan tetap 200,
layar tetap menampilkan keberhasilan, dan datanya lenyap.

---

## 2. Temuan

### 2.1 Field yang hilang

| Kelompok | Field | Rumah yang benar |
| --- | --- | --- |
| Tanda vital | `systolicBloodPressure`, `diastolicBloodPressure`, `pulseRate`, `respiratoryRate`, `temperature`, `oxygenSaturation` | `TrxPatientVitalSign` |
| Keluhan | `chiefComplaint` | `TrxEmergencyVisit.ChiefComplaint` |
| Riwayat | `currentIllnessHistory`, `pastMedicalHistory`, `medicationHistory`, `allergyHistory` | `TrxPatientMedicalHistory`, `TrxPatientAllergy` |
| Tindak lanjut | `referredTo`, `sentTo`, `conclusion` | lihat bagian 3 |
| Lain-lain | `triagedAt`, `emergencyTriageLevelId` | tidak ada padanan |

Yang tersimpan hanya `triageLevelId`, `triageStatus`, `notes`, dan `isActive`.

**Tanda vital adalah yang paling berbahaya.** Nadi, tekanan darah, dan saturasi adalah dasar
keputusan triase. Ketiadaannya membuat penilaian tidak dapat ditinjau ulang maupun diaudit.

### 2.2 Kesimpulan pengkajian wajib diisi tetapi dibuang

Field `conclusion` ditandai `required` pada formulir. Perawat tidak dapat menyimpan tanpa
mengisinya, lalu isinya hilang. Ini bentuk kegagalan paling menyesatkan: sistem memaksa
petugas melakukan sesuatu yang tidak berpengaruh apa pun.

### 2.3 Dua konsep berbeda yang tampak sama

Pertanyaan awal owner beralasan. "Diteruskan Kepada" dan "dokter pemeriksa" terlihat serupa
padahal berbeda sifat:

| | "Diteruskan Kepada" | Dokter pemeriksa |
| --- | --- | --- |
| Bentuk | Teks bebas | Rujukan ke master dokter |
| Tersimpan | Tidak | `TrxPatientEncounter.DoctorId` |
| Dapat ditelusuri | Tidak | Ya |

---

## 3. Keputusan yang diambil

Prinsipnya satu: **arahkan setiap data ke pemiliknya yang sudah ada, jangan menambah kolom
baru.** Menambahkan 17 kolom ke `TrxEmergencyTriage` akan menduplikasi tanda vital yang sudah
dimiliki `TrxPatientVitalSign` dan keluhan yang sudah dimiliki `TrxEmergencyVisit`. Dua tempat
menyimpan fakta yang sama akan berbeda isi cepat atau lambat, dan pada data klinis itu
berbahaya.

| No | Data | Keputusan |
| ---: | --- | --- |
| 1 | Tanda vital | Disimpan ke `TrxPatientVitalSign` lebih dulu, ditautkan lewat `PatientVitalSignId` |
| 2 | Keluhan utama | Dipetakan ke `TriageReason`; sumber utamanya tetap `TrxEmergencyVisit` saat pendaftaran |
| 3 | Kesimpulan pengkajian | Digabung ke `Notes` bersama catatan perawat, dengan penanda tekstual |
| 4 | `referredTo` | Dihapus, digantikan penetapan dokter yang tersimpan sebagai relasi |
| 5 | `sentTo` | Dihapus; tujuan perpindahan dimiliki `TrxEmergencyTransfer` sesuai `IGD-DEC-042` |
| 6 | Riwayat penyakit | Rumahnya sudah ada: `TrxPatientMedicalHistory` dan `TrxPatientAllergy` — **belum disambungkan** |

---

## 4. Perubahan

### 4.1 Backend

**Tidak ada perubahan skema dan tidak ada migration.** Kontrak
`CreateEmergencyTriageRequest` sudah menyediakan `PatientVitalSignId`, dan seluruh entitas
tujuan sudah ada beserta endpoint-nya.

Dua perubahan perilaku kecil dari sesi yang sama:

| Berkas | Perubahan |
| --- | --- |
| `EmergencyTriageController.cs` | `CompletedAt` diisi saat penilaian dibuat dalam keadaan selesai, menyamakan dengan jalur ubah status |
| `PatientEncounterController.cs` | Endpoint baru `PATCH /patient-encounters/{id}/doctor` |
| `PatientEncounterDtos.cs` | `PatientEncounterAssignDoctorRequest` |

### 4.2 Frontend

| Berkas | Perubahan |
| --- | --- |
| `emergency-management-triage-slice.jsx` | Thunk `saveEmergencyPatientVitalSign`, `fetchEmergencyDoctorOptions`, `assignEmergencyEncounterDoctor` |
| `emergency-management-triage-utils.jsx` | Payload triage dibersihkan menjadi hanya properti yang dikenal kontrak; helper `buildTriageNotes` |
| `use-emergency-management-triage-form.jsx` | Tanda vital disimpan lebih dulu, hasilnya ditautkan ke penilaian |
| `emergency-triage-follow-up-section.jsx` | `referredTo` dan `sentTo` dihapus |
| `use-emergency-triage-doctor.jsx` | **Baru** — hook penetapan dokter |
| `emergency-triage-doctor-section.jsx` | **Baru** — tampil setelah penilaian tersimpan |
| `emergency-management-triage-constant.jsx` | `EMERGENCY_TRIAGE_STATUS`; nilai bawaan yatim dibuang |

### 4.3 Tiga blokir lain yang ditemukan pada review yang sama

Ketiganya harus benar bersamaan agar status pasien berubah, dan sebelumnya tidak satu pun
terpenuhi:

1. `emergencyVisitId` selalu kosong karena konteks pasien diambil dari `/patient-encounters`
   yang tidak memiliki field itu. Sekarang diambil dari `GET /emergency-visits`.
2. `triageStatus` tidak dikirim, sehingga backend menyimpannya sebagai `Draft`. Penilaian
   `Draft` tidak pernah mengubah kunjungan menjadi `Triaged`. Sekarang dikirim `Completed`.
3. `CompletedAt` tidak terisi pada penilaian yang langsung selesai.

---

## 5. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint seluruh berkas tersentuh | Bersih |
| Unit test peta status kunjungan | 6 lulus, 0 gagal |
| Halaman triage dirender dev server | HTTP 200 |
| Kompilasi backend | 0 error C# |
| **Alur simpan dijalankan sungguhan** | **Belum** |
| **Tanda vital terbukti tersimpan di basis data** | **Belum** |

Status keseluruhan tetap **belum terbukti**. Yang dibuktikan baru bahwa kode terkompilasi,
lolos lint, dan halaman dirender.

---

## 6. Yang belum dikerjakan

| No | Hal | Alasan |
| ---: | --- | --- |
| 1 | Riwayat penyakit dan alergi belum disambungkan | Rumahnya sudah ada (`TrxPatientMedicalHistory`, `TrxPatientAllergy`), tetapi pemetaan field dan aturan pengisiannya belum ditetapkan owner |
| 2 | Keluhan utama masih diinput ulang di triage | Sebaiknya ditampilkan dari kunjungan, bukan diketik ulang; perlu perubahan tampilan |
| 3 | `FE-IGD-006` penyelesaian kunjungan | Menunggu halaman pengkajian pasien IGD |
| 4 | Nosokomial | **Tidak ada entitasnya di seluruh repo**; butuh desain, bukan pengetikan kode |

---

## 7. Koreksi atas pernyataan sebelumnya

Pada percakapan sebelumnya saya menyatakan **assessment klinis tidak ada di repo**. Itu
keliru. `TrxPatientAssessment` beserta controller dan DTO-nya ada di Clinical Management;
saya hanya membaca hasil pencarian teratas yang kebetulan seluruhnya milik modul Human
Resource.

Yang benar-benar tidak ada hanyalah **nosokomial** — nol berkas di seluruh repository.

Akibat kekeliruan itu, rekomendasi awal saya soal pengkajian pasien terlalu pesimistis:
sebagian besar rumahnya ternyata sudah tersedia.

---

## 8. Risiko tersisa

| No | Risiko | Keadaan |
| ---: | --- | --- |
| 1 | Data lama triage tetap kosong | Penilaian yang terlanjur tersimpan tanpa tanda vital tidak dapat dipulihkan; datanya memang tidak pernah terkirim |
| 2 | Riwayat penyakit masih hilang | Field-nya masih ada di formulir tetapi belum disambungkan ke endpoint tujuannya |
| 3 | Penggabungan kesimpulan ke `Notes` | Keduanya menjadi satu teks; bila kelak perlu dipisah, diperlukan kolom tersendiri |
| 4 | Belum ada test otomatis untuk alur simpan | Solution backend tidak memiliki test project |
