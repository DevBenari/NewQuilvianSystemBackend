# Laporan Perubahan Frontend — `FE-HRD-R31`

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `FE-HRD-R31` — Layanan SDM bagian 2: ESS, surat, verifikasi publik |
| Task asal | `FE-HRD-178`–`184` |
| Slice | `S-D4`, Gelombang 3 |
| Kontrak | API Contract v17 bagian 23 dan source backend branch `MHamzah` |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Branch frontend | `HamzahV2` (`origin/HamzahV2`) |
| Tanggal | 13 November 2026 |
| Status | 🟡 Sebagian — alur utama nyata; dua kebutuhan tertahan kontrak dan uji browser belum tersedia |

## Hasil perubahan

- Portal ESS mengambil kategori, jenis layanan, SLA, dan riwayat tiket aktual. Submit mengirim DTO backend yang tepat, memvalidasi subjek/uraian, memaksa nomor disposisi Cito, lalu mengunggah PDF/JPG/PNG/DOCX maksimal 5 MB setelah tiket berhasil dibuat.
- Meja bantuan yang sudah dikerjakan pada R19 dipertahankan: antrean, claim/re-assign, perubahan status, komentar, dan catatan internal memakai API nyata.
- Route baru `/corporate/hrd/services/documents` menampilkan antrean penerbitan dan menyediakan aksi permission-gated untuk approval C&B, otorisasi final, penerbitan PIN satu kali, dan revokasi ber-BAP.
- Verifikasi publik `/verify/document/[hash]` tidak lagi memakai dokumen atau PIN contoh. Hash diverifikasi anonim, metadata finansial tetap termasking, dan unlock mengirim `verificationHash` serta `securityPin` sesuai DTO backend.
- Client request dokumen diperbaiki agar `workforceProfileId` dikirim sebagai query parameter. Unlock PIN yang sebelumnya mengirim field `{ hash, pin }` diperbaiki ke nama kontrak backend.
- Lima komponen mock lama yang sudah tidak dipakai dihapus: wizard semu, drawer semu, panel diff semu, serta modal otorisasi dan revokasi semu. Implementasi aktif memakai base components dan data server.
- Menu Layanan SDM diberi permission `HrServiceTicket:Read`, `HrServiceCatalog:Read`, dan `EmployeeDocumentIssuance:Read`.

## Keputusan UI

`UI GATE: REUSE/COMPOSE`. Implementasi memakai `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `BaseButton`, `ConfirmModal`, keluarga `BaseFormControl`, `InformationAlert`, `ToastStack`, `HrStatusBadge`, dan `HrInfoGrid`. Tidak ada base component baru dan tidak ada inline style pada empat view aktif R31.

## Gap kontrak dan penerimaan

| Task asal | Verdict | Catatan |
| --- | --- | --- |
| `FE-HRD-178` | 🟡 | Katalog dan SLA nyata; pola kartu dengan klik langsung diganti pemilih di dialog |
| `FE-HRD-179` | 🟡 | Submit, validasi, Cito, dan upload nyata; bukan wizard/drag-and-drop |
| `FE-HRD-180` | 🟡 | Klarifikasi dan catatan internal nyata; preview lampiran side-by-side dan transisi status otomatis dari komentar belum ada |
| `FE-HRD-181` | ⛔ | Backend menyinkronkan profil saat status `Resolved`, tetapi DTO tiket tidak membawa diff lama/baru untuk verifikasi pra-resolve |
| `FE-HRD-182` | 🟡 | Approval, otorisasi, anti-input nominal, PIN/hash/QR nyata; tidak ada endpoint preview PDF draf dan UI tidak dapat memprediksi anti-self-approval |
| `FE-HRD-183` | 🟡 source selesai | Nomor BAP, alasan minimal 20 karakter, aksi revokasi, dan refresh status terpasang; belum diuji browser/peran |
| `FE-HRD-184` | 🟡 source selesai | Verifikasi anonim, banner legalitas, masking, PIN, dan pesan limiter terpasang; belum diuji browser/mobile |

## Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint target R31, service, route, dan menu | PASS — 0 error/warning |
| CSS usage gate | PASS — 4 view aktif, 0 inline style/import yatim |
| `npm run build` | PASS — 814 halaman dan standalone runtime siap |
| Uji browser/peran/mobile | `VISUAL CHECK: NOT FEASIBLE` — tool browser tidak tersedia |

## Catatan keamanan

- Nominal finansial tidak pernah menjadi input otorisasi.
- PIN hasil generate hanya ditampilkan dari respons penerbitan saat sesi halaman aktif dan tidak direka ulang.
- Batas salah PIN dan lockout tetap diputuskan backend; frontend hanya menampilkan pesan server.
- Anti-self-approval tetap ditegakkan backend sebagai sumber kebenaran.

Perubahan belum di-commit atau di-push.
