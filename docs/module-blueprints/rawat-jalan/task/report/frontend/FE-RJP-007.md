# Laporan Task Frontend — `FE-RJP-007` Tiket kiosk dan kartu antrean

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-007`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-007` |
| Slice | `MVP-1`, `EPIC RP-07` |
| Trace | `FR-RP-060`, `061`; `RJ-PRI-DEC-017`, `018`; `R-RP-21` (tiket), `R-RP-29`, `UAT-RP-16`, `UAT-RP-17` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0`: respons Patient Encounter `queueClassification.priorityWaitingLocationName`; item Nurse Station Queue dan Doctor Queue `priorityWaitingLocationName` |
| Dependency | `BE-RJP-011` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 19 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 4/4 `PASS` (kiosk, Nurse Station, Klinis Dokter), lint dan build `PASS` |

## 2. Proses bisnis

Pasien prioritas tidak menunggu di ruang tunggu poli. Nama ruang tunggunya diambil dari konfigurasi backend
(`HealthServices:Registration:PriorityReservation`), bawaan "Ruang Tunggu Prioritas". Frontend tidak menulis nama
ruangan sendiri.

| Tempat | Pasien prioritas | Pasien Reguler |
| --- | --- | --- |
| Tiket kiosk | Pemberitahuan "Silakan menunggu di Ruang Tunggu Prioritas. Petugas akan menghampiri Anda." | Tanpa pemberitahuan |
| Kartu antrean Nurse Station | Lencana "Prioritas" (dan "Member"), lalu baris "Ruang Tunggu Prioritas" | Tanpa penanda lokasi |
| Kartu antrean Klinis Dokter | Sama dengan Nurse Station | Tanpa penanda lokasi |

Contoh: pasien Gold mendaftar sendiri di kiosk dan masih punya reservasi terbuka hari itu. Tiket menampilkan arahan ke
ruang tunggu prioritas, dan reservasinya berubah menjadi **Didaftarkan lewat Jalur Lain**. Perawat melihat kartu
`I105` bertanda Prioritas dengan lokasi Ruang Tunggu Prioritas, lalu menghampiri pasien di sana.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/components/view/kiosk/registration/shared/kiosk-referral-ticket-notice.jsx` | Diperbarui | Prop opsional `message`. Tanpa `message`, teks bawaan surat rujukan tidak berubah |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-ticket.jsx` | Diperbarui | Membaca `queueClassification.priorityWaitingLocationName` (hanya bila `isPriorityQueue`) dan menampilkan pemberitahuan |
| `src/components/features/health-services/nurse-station-management/QueuePatientCard.jsx` | Diperbarui | Baris lokasi memakai kelas `queueMetaLine` yang sudah ada, di bawah lencana Prioritas/Member |
| `src/components/features/health-services/doctor-queue-features/QueuePatientCard.jsx` | Diperbarui | Sama dengan Nurse Station |
| `src/utils/health-services/registration-management/nurse-station-management/nurse-station-queue-display-utils.js` | Diperbarui | `getPriorityWaitingLocationName(item)` |
| `src/utils/health-services/registration-management/doctor-queue/doctor-queue-display-utils.js` | Diperbarui | `getPriorityWaitingLocationName(item)` |

Perbaikan pada akhir sesi: versi pertama menaruh nama lokasi di antara lencana "Prioritas" dan "Member". Pada kartu
selebar ±270 px, teks itu terpecah menjadi tiga baris. Nama lokasi dipindah ke baris sendiri dengan kelas
`queueMetaLine` yang sudah ada di kedua stylesheet. Build dan uji layar dijalankan ulang.

## 4. Gerbang komponen dasar

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Pemberitahuan di tiket kiosk | `EXTEND` `KioskReferralTicketNotice` dengan prop opsional `message`; perilaku bawaan tidak berubah | Lihat pilihan 1 |
| Baris lokasi di kartu antrean | `REUSE` kelas `queueMetaLine` milik stylesheet Nurse Station dan Doctor Queue | `nurse-station-queue-view.module.css`, `doctor-queue-view.module.css` |
| Lencana Prioritas/Member | `REUSE` (tidak diubah) | `QueuePatientCard.jsx` |

**Pilihan 1 — pemberitahuan di tiket kiosk**

1. Tambah prop opsional `message` pada `KioskReferralTicketNotice` (Direkomendasikan, dipakai) — rupa sama dengan
   pemberitahuan surat rujukan yang sudah dikenal pasien di kiosk. Tanpa `message` teksnya tetap sama, jadi tiket
   rujukan tidak berubah. Biaya: 3 baris.
2. Komponen pemberitahuan baru khusus prioritas — tidak menyentuh komponen lama, tetapi menduplikasi rupa yang sama dan
   menambah satu komponen untuk dirawat.

Pilihan 1 adalah `EXTEND` yang **tidak** mengubah perilaku bawaan, sehingga tidak menunggu keputusan user.

`UI GATE: PASS — REUSE dan satu EXTEND berprop opsional (default tidak berubah); tidak ada NEW.`

Checklist konsistensi UI: tidak ada CSS baru, warna literal, `<button>` mentah baru, atau inline style baru pada baris
yang ditambahkan.

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (build akhir 19 Oktober 2026) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2976/3003; 27 gagal di modul lain, sudah gagal pada snapshot `HEAD`. Tidak ada uji unit baru untuk task ini (opsional) |
| `MANUAL TEST` (Playwright, build standalone port 3000, backend uji 7185) | `PASS` 4/4, rincian di bawah |

### Uji layar

Data samaran: pasien Gold `RJPR UJI FE-KIOSK …` dengan reservasi terbuka hari ini, pasien Reguler `FE-REGQ …`
(antrean `I024`), pasien Gold `FE-LINK …` (antrean `I100`, dipindah ke tahap dokter lewat API panggil → mulai →
selesai skrining). Kiosk memakai perangkat kiosk uji dan akun login kiosk yang dibuat lewat API master data kiosk.

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| **`R-RP-21` tiket** | Kiosk pasien Gold: tiket menampilkan "Silakan menunggu di Ruang Tunggu Prioritas"; reservasi terbuka menjadi Jalur Lain (`status=6`) | `PASS` |
| **`UAT-RP-16`** | Nurse Station: kartu `I105` menampilkan "Prioritas" dan "Ruang Tunggu Prioritas" | `PASS` |
| **`UAT-RP-17`** | Nurse Station: kartu Reguler `I024` tanpa penanda lokasi | `PASS` |
| **`R-RP-29`** dokter | Klinis Dokter: kartu `I100` menampilkan "Ruang Tunggu Prioritas" | `PASS` |

Uji kartu memakai akar kartu `<article>` dan menggulir daftar antrean (infinite scroll) sampai kartu ditemukan. API
juga diperiksa langsung: item prioritas membawa `priorityWaitingLocationName = "Ruang Tunggu Prioritas"`, item Reguler
`null`.

Tangkapan layar: `fe-rjp-007-kiosk-ticket.png`, `fe-rjp-007-nurse-priority.png`, `fe-rjp-007-nurse-regular.png`,
`fe-rjp-007-doctor.png` di scratchpad sesi.

## 6. Delta dan risiko

| Butir | Catatan |
| --- | --- |
| `R-RP-21` tanpa reservasi | Uji layar hanya memakai pasien Gold yang punya reservasi terbuka. Pasien Gold tanpa reservasi memakai jalur tampilan yang sama (pemberitahuan bergantung pada `isPriorityQueue` + nama lokasi, bukan pada reservasi). Klasifikasi tanpa reservasi dibuktikan di backend (`BE-RJP-011`) |
| Kiosk | Task ini menyentuh layar kiosk. Pemilik kiosk perlu diberi tahu bahwa tiket pasien prioritas kini memuat arahan ruang tunggu |
| Data uji | Perangkat kiosk uji dan perangkat display uji tetap ada di DB `QuilvianNewDevSukma` sebagai data samaran |

## 7. Wewenang dan batas

- Nama ruang tunggu tidak ditulis di frontend; selalu dari backend.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.
