# Laporan Perubahan Backend — `BE-CRM-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-020` |
| Judul | Proses terjadwal |
| Slice | `MVP-2`, `EPIC CRM-03` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-020` |
| Trace | `FR-CRM-020` s.d. `FR-CRM-023`; `CRM-DEC-005`, `CRM-DEC-014`, `CRM-DEC-024`; state `v1`; integrasi `INT-08` |
| Contract version | State `v1`, integrasi `v1`, `approved` |
| Dependency | `BE-CRM-011` 🟡, `BE-CRM-012` ✅ |
| Klasifikasi | `MEDIUM`: satu perintah sistem di service, satu hosted service + options, registrasi, 11 uji |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/Membership/Services/`, `Program.cs`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `83ef27bd` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. Dijalankan di uji SQLite. Belum dijalankan terhadap devYoga |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (`Pat`, `ACTIVE`) |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-TXN-001` (satu pasien satu transaksi), `QBE-LOG-001` (ringkasan per putaran dan galat), `QBE-AUD-001` (setiap perubahan tercatat di riwayat dengan sumber `System`) |

## 1. Masalah yang diperbaiki

Tanpa proses harian, membership yang habis tetap Aktif, perpanjangan yang sudah dibayar tidak
pernah mulai, dan pendaftaran yang tidak dibayar menggantung selamanya. Selain itu, 14 pasien di
devYoga bertanda member padahal tidak punya membership (nilai bawaan lama `IsMember = true`).

## 2. Proses bisnis

**Pemicu:** saat aplikasi mulai (mengejar hari yang terlewat), lalu setiap hari pukul 00.05 WIB
(`PatientMembership:Lifecycle:RunAtLocalTime`). Bawaannya **menyala**
(`PatientMembership:Lifecycle:Enabled = true`). Hanya berjalan pada runtime role yang menjalankan
background job (`runBackgroundJobs`), sama seperti penjadwal lain.

**Langkah setiap putaran:**

1. **Kedaluwarsa:** Aktif dengan tanggal akhir **sebelum** hari ini (WIB) menjadi Kedaluwarsa.
2. **Mulai Terjadwal:** bila periode lama sudah berhenti Aktif, perpanjangan Terjadwal yang tanggal mulainya sudah tiba menjadi Aktif, dalam transaksi yang sama. Kedaluwarsa disimpan lebih dulu, karena index unik "satu Aktif per pasien" diperiksa per baris.
3. **Batal tidak dibayar:** Menunggu Pembayaran yang lewat batas bayar menjadi Dibatalkan (kategori `Unpaid`), dengan riwayat `UNPAID_CANCELLED` dan `FEE_CHARGE_CANCEL_REQUESTED`. Statusnya diperiksa ulang di bawah kunci pasien, untuk berjaga kalau sudah dibatalkan petugas.
4. **Sapu proyeksi:** pasien yang bertanda member, atau yang punya membership Aktif, diselaraskan dengan aturan tunggal J1.

Setiap perubahan menulis riwayat bersumber `System` tanpa pelaku.

| Contoh | Hasil |
| --- | --- |
| Gold berakhir 12 Okt 2027. Putaran 13 Okt 2027 00.05 WIB | Kedaluwarsa. Pasien Reguler |
| Gold berakhir 12 Okt 2027 + perpanjangan Terjadwal 13 Okt 2027 s.d. 12 Okt 2028 | Lama Kedaluwarsa, baru Aktif, status member pasien menunjuk yang baru |
| Server mati 13–14 Okt, menyala 15 Okt | Putaran saat start memproses keduanya |
| Pendaftaran dengan batas bayar 12 Okt 09.00 | 13 Okt dibatalkan "tidak dibayar" |
| Pukul 23.30 WIB di hari terakhir | Tidak diproses, karena masih hari terakhir |

**Jalur tidak normal:** galat pada satu pasien dicatat (`LoggerService.ErrorAsync`, tanpa data
sensitif) dan tidak menahan pasien lain. Jumlahnya masuk `Failures`, lalu dicoba lagi pada
putaran berikutnya.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipService.cs` | `RunDailyLifecycleAsync`, `MembershipLifecycleRunResult`, bantuan `AdvancePeriodsAsync`, `CancelUnpaidAsync`, `SweepPatientProjectionsAsync`, `RunGuardedAsync`, `AddSystemHistory` |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipLifecycleHostedService.cs` | Baru: hosted service + `PatientMembershipLifecycleOptions` + `DelayUntilNextRun` |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipStatusResolver.cs` | Properti `TimeZone` (zona kalender membership) |
| `Program.cs` | `Configure<PatientMembershipLifecycleOptions>` + `AddHostedService` di blok `runBackgroundJobs` |
| `Tests/.../MembershipLifecycleAndKioskTests.cs` | Baru (bersama `BE-CRM-021`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Menulis `MstPatientMembership`, `PatMembershipStatusHistory`, dan proyeksi `MstPatient`. Tanpa migration |
| Keamanan/Auth | `NOT APPLICABLE` (proses sistem). Log ringkasan hanya berisi angka |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Aktif lewat tanggal akhir (`AT-08`) | Kedaluwarsa, riwayat `EXPIRED` sumber `System` tanpa pelaku, proyeksi kosong | `PASS` | Uji SQLite |
| Hari terakhir pukul 23.30 WIB | Tidak berubah | `PASS` | idem |
| Lama berakhir + Terjadwal mulai | Lama Kedaluwarsa, baru Aktif, proyeksi pindah | `PASS` | idem |
| Terjadwal belum waktunya | Tidak disentuh | `PASS` | idem |
| Server mati dua hari | Dikejar | `PASS` | idem |
| Menunggu lewat batas (`AT-06`) / belum lewat | Dibatalkan `Unpaid` + dua riwayat / tetap | `PASS` | idem |
| Dijalankan dua kali | Putaran kedua nol perubahan, riwayat tidak bertambah | `PASS` | idem |
| Sapu proyeksi | Member palsu dikosongkan, member sah diisi | `PASS` | idem |
| `DelayUntilNextRun` | 09.00 → 15 j 5 m; 00.01 → 4 m; 00.05 → 24 j | `PASS` | idem |
| DI membuat hosted service, `Enabled` bawaan `true` | Berhasil | `PASS` | idem |
| `dotnet test` seluruh project | 166 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |
| Putaran sungguhan di devYoga (startup backend) | — | `NOT RUN` | Menulis data bersama butuh izin. Yang akan terjadi saat backend dijalankan: 14 pasien `IsMember = true` tanpa membership dikosongkan |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-06`, `AT-08` lulus | Terpenuhi | Uji di atas |
| Menjalankan dua kali tidak menambah riwayat | Terpenuhi | Uji "dijalankan dua kali" |
| Data Terjadwal buatan menjadi Aktif tepat pada hari H | Terpenuhi | Uji "lama berakhir + Terjadwal mulai" |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Begitu backend dijalankan dengan role yang menjalankan background job, putaran pertama langsung menulis ke database** yang dipakai, termasuk mengosongkan 14 tanda `IsMember` palsu di devYoga. Itu perilaku yang diinginkan (`CRM-CAP-004`), tetapi perlu Anda ketahui. Untuk menahannya, atur `PatientMembership:Lifecycle:Enabled = false` |
| Masalah yang diketahui | Urutan "kedaluwarsa dulu baru Terjadwal aktif" hanya terbukti di SQLite, karena uji SQLite melepas index unik berfilter. Di PostgreSQL urutannya dijamin oleh dua `SaveChanges` terpisah dalam satu transaksi |
| Risiko tersisa | Kasus tepi `EPIC CRM-08`: perpanjangan yang lunas setelah periode lama berakhir. Saat itu tanggal mulainya sudah lewat, sehingga proses ini akan langsung mengaktifkannya di putaran berikutnya |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Berkas uji baru `MembershipLifecycleAndKioskTests.cs` diabaikan git, perlu `git add -f` |
| Langkah berikutnya | `BE-CRM-015` + `FE-CRM-001` (rilis berpasangan) |
