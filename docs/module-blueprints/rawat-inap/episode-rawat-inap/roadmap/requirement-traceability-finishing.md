# Traceability Requirement — Episode Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Berkas | `episode-rawat-inap/roadmap/requirement-traceability-finishing.md` — revision `1` |
| Status | **`DRAFT`**, mengikuti status kedua roadmap pendamping |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `episode-rawat-inap`, kontrak `0.10.0` `approved` 2026-10-02 (`RWI-DEC-221`) |
| Roadmap | `backend-roadmap-finishing.md` revision `1` (`BE-RWI-172` s.d. `184`); `frontend-roadmap-finishing.md` revision `1` (`FE-RWI-192` s.d. `201`) |
| Sumber requirement | `PRD-RWI-FINISHING-001` v`0.4` (rumusan FR); `04-prd-to-mvp.md` bagian 23; decision log revision `32` |
| Masukan dan hash | Seperti metadata roadmap; hash lengkap pada `../blueprint-manifest.md` bagian 11 |
| Source SHA | Backend `bf5c6bde`; frontend `f74758af5` |
| Gate | `evidence/02-requirement-completeness-gate.md` revision `1.10` bagian 19 |

**Cara membaca bukti.** Backend: verifikasi API/kontrak, pencarian kode, verifikasi proses bisnis, dan runtime bila tersedia — bukan automated test (`rules/backend/TEST_POLICY.md`). Frontend: lint, build, dan verifikasi manual kontrol interaktif. Butir yang tidak dijalankan ditulis `NOT RUN`.

**Pembaruan bukti 5 Oktober 2026.** Build project saat `dotnet ef database update` **PASS** menurut output pengguna yang diterima 5 Oktober 2026 (`Build succeeded.`); migration `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.`. Uji API, regresi, alur klinis, dan rollback `Down()` tetap `NOT RUN`. Nama database dan lingkungan tidak tercantum pada output. Catatan pengecualian 2 Oktober 2026 adalah riwayat sesi implementasi, bukan status build/migration terkini. Penerapan mencakup enam task pemilik perubahan skema, bukan semua task Finishing. Requirement dengan frontend/runtime tertunda tetap sebagian. `BE-RWI-149` mempertahankan catatan pengemasan `I1`/`I2` menjadi satu migration. SHA metadata tetap snapshot perencanaan; approval roadmap tetap `DRAFT`. [Bukti penerapan](../task/report/backend/BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026).

## 1. Matriks requirement → desain → kontrak → task → bukti

| Requirement | Keputusan | Desain | Kontrak `0.10.0` | Task backend | Task frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|---|
| `FR-RWF-040` Dua tab Pemesanan Ruangan Bedah | `RWI-DEC-175` | FE 13.4.1 | API 11.2 | `BE-RWI-175` | `FE-RWI-193` | Manual dua tab; source: [BE-RWI-175](../task/report/backend/BE-RWI-175.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-041` Tepat satu order tindakan operasi aktif | `RWI-DEC-176` (1) | Backend 12.3 (`INV-RWF-25`) | API 11.2 | `BE-RWI-175` | `FE-RWI-193` | Tanpa order → `INP-SRG-001` (`AC-RWF-040`, `UAT-RWF-32`); source: [BE-RWI-175](../task/report/backend/BE-RWI-175.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-042` Isian minimal pemesanan | `RWI-DEC-176` | API 11.2 | API 11.2 | `BE-RWI-174`, `BE-RWI-175` | `FE-RWI-193` | Verifikasi API dan manual; source: [BE-RWI-174](../task/report/backend/BE-RWI-174.md); [BE-RWI-175](../task/report/backend/BE-RWI-175.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-043` Bedah Obgyn berjenis Obstetri | `RWI-DEC-175` | Backend 12.7 | API 11.2, 11.5.1 | `BE-RWI-174`, `BE-RWI-175` | `FE-RWI-193` | `UAT-RWF-05` (jenis Obstetri); source: [BE-RWI-174](../task/report/backend/BE-RWI-174.md); [BE-RWI-175](../task/report/backend/BE-RWI-175.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-044` Delapan label status termasuk Ditolak | `RWI-DEC-204` | FE 13.4.2 | API 11.5.1 | `BE-RWI-174` | `FE-RWI-194` | Manual (`AC-RWF-041`); source: [BE-RWI-174](../task/report/backend/BE-RWI-174.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-045` Catatan Pra-Operasi dua sisi | `RWI-DEC-173` | Backend 12.3 (`INV-RWF-26`) | API 11.3, 11.4 | `BE-RWI-172`, `173`, `176` | `FE-RWI-192`, `FE-RWI-195` | Dua akun (`AC-RWF-042`, `045`, `046`); source: [BE-RWI-172](../task/report/backend/BE-RWI-172.md); [BE-RWI-173](../task/report/backend/BE-RWI-173.md); [BE-RWI-176](../task/report/backend/BE-RWI-176.md); API/runtime `NOT RUN` | 🟡 Source backend ada; migration `BE-RWI-172` sudah diterapkan menurut output pengguna; frontend dan runtime belum diverifikasi |
| `FR-RWF-046` Serah terima pasca operasi oleh penerima sah | `RWI-DEC-177`, `189` | Backend 12.3 (`INV-RWF-28`) | API 11.5.2, 11.5.3 | `BE-RWI-177` | `FE-RWI-196` | `UAT-RWF-13` (`AC-RWF-043`, `047`); source: [BE-RWI-177](../task/report/backend/BE-RWI-177.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-047` Biaya operasi saat `Completed`, dua sumber tanpa tumpang tindih | `RWI-DEC-192`, `196` | Backend 12.3 (`INV-RWF-29`, `30`) | Backend 12.7; API 11.9 | `BE-RWI-172`, `178`, `179` (bergantung `integrasi-billing` `BE-RWI-155`) | Formulir tarif komponen: `keperawatan` `FE-RWI-187` | Proses bisnis dengan Billing sungguhan (`AC-RWF-044`, `092`, `099`); `UAT-RWF-05`; source: [BE-RWI-172](../task/report/backend/BE-RWI-172.md); [BE-RWI-178](../task/report/backend/BE-RWI-178.md); [BE-RWI-179](../task/report/backend/BE-RWI-179.md); API/runtime `NOT RUN` | 🟡 Source backend ada; migration `BE-RWI-172` sudah diterapkan menurut output pengguna; frontend dan runtime belum diverifikasi |
| `FR-RWF-048` Penandaan area operasi tanpa foto | `RWI-DEC-174` | Backend 12.3 (`INV-RWF-27`) | API 11.3 | `BE-RWI-176` | `FE-RWI-195` | Sisi berbeda → `OPR-WPO-001`; source: [BE-RWI-176](../task/report/backend/BE-RWI-176.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-049` Bed tetap selama operasi | `RWI-DEC-177` (1) | Backend 12.5 | API 11.5.2 | `BE-RWI-177` | `FE-RWI-196` | Menerima tidak memindahkan bed (`AC-RWF-049`); source: [BE-RWI-177](../task/report/backend/BE-RWI-177.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-090` Pra-operasi setelah penundaan | `RWI-DEC-199` | Backend 12.5 | API 11.3 | `BE-RWI-176` | `FE-RWI-195` | `UAT-RWF-21` (`AC-RWF-093`, `094`); source: [BE-RWI-176](../task/report/backend/BE-RWI-176.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-080` Permintaan admisi dari kamar pulih | `RWI-DEC-201`, `208` | Backend 12.3 (`INV-RWF-32`) | API 11.6, 11.5.2 | `BE-RWI-181` | `FE-RWI-198` | `UAT-RWF-16` (`AC-RWF-080`); source: [BE-RWI-181](../task/report/backend/BE-RWI-181.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-089` Batas permintaan admisi | `RWI-DEC-201`, `220` (1) | Backend 12.3 | API 11.6 | `BE-RWI-181` | `FE-RWI-198` | `UAT-RWF-33`, `41` (`AC-RWF-089`); source: [BE-RWI-181](../task/report/backend/BE-RWI-181.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-081` Ringkasan operasi di bangsal | `RWI-DEC-197`, `213` | Backend 12.7 | API 11.5.1 | `BE-RWI-180` | `FE-RWI-196`; dokter: `dokter-rawat-inap` `FE-RWI-178` | `UAT-RWF-17` (`AC-RWF-081`); source: [BE-RWI-180](../task/report/backend/BE-RWI-180.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-082` Akses ringkasan dan laporan draft | `RWI-DEC-197` | Backend 12.7 | API 11.5.1 | `BE-RWI-180` | `FE-RWI-196` | Laporan draft → "belum final" (`AC-RWF-082`); source: [BE-RWI-180](../task/report/backend/BE-RWI-180.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-086` Order operasi ditolak | `RWI-DEC-204`, `208` | Backend 12.3 (`INV-RWF-31`) | API 11.5.1, 11.9 | `BE-RWI-174` | `FE-RWI-194`, `FE-RWI-197` | `UAT-RWF-24` (`AC-RWF-085`, `099`); source: [BE-RWI-174](../task/report/backend/BE-RWI-174.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-088` Serah terima pasca operasi tertunda di daftar pantau | `RWI-DEC-201`, `216`, `220` (6) | Backend 12.12 | API 11.5.3, 11.9 | `BE-RWI-172`, `177`, `182` | `FE-RWI-199` | Data melewati ambang (`AC-RWF-087`); source: [BE-RWI-172](../task/report/backend/BE-RWI-172.md); [BE-RWI-177](../task/report/backend/BE-RWI-177.md); [BE-RWI-182](../task/report/backend/BE-RWI-182.md); API/runtime `NOT RUN` | 🟡 Source backend ada; migration `BE-RWI-172` sudah diterapkan menurut output pengguna; frontend dan runtime belum diverifikasi |
| `FR-RWF-087` Laporan transfer ruangan (`P2`) | `RWI-DEC-205`, `214`, `215`, `220` (4) | Backend 12.3 (`INV-RWF-34`) | API 11.7 | `BE-RWI-184` (bergantung `integrasi-billing` `BE-RWI-154`) | `FE-RWI-201` | `UAT-RWF-20`, `44` (`AC-RWF-086`, `100`, `RWI-AC-340`); source: [BE-RWI-184](../task/report/backend/BE-RWI-184.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `FR-RWF-071` Serah terima klinis transfer (`P2`) | `RWI-DEC-182`, `189` | Backend 12.3 (`INV-RWF-33`) | API 11.8 | `BE-RWI-183` | `FE-RWI-200` | `UAT-RWF-14` (`AC-RWF-071`, `072`); source: [BE-RWI-183](../task/report/backend/BE-RWI-183.md); API/runtime `NOT RUN` | 🟡 Backend selesai kode; frontend dan bukti runtime belum tersedia |
| `RWI-DEC-207` Kunjungan asal dibaca Billing dari `InpAdmissionReferral` | `RWI-DEC-207` | Backend 12.15 | API 11.11 | `BE-RWI-181` (sumber); tautan di `integrasi-billing` `BE-RWI-159` | — | Pesan `ADMISSION_CONFIRMED` tetap daftar putih; source: [BE-RWI-181](../task/report/backend/BE-RWI-181.md); API/runtime `NOT RUN` | 🟡 Sumber `BE-RWI-181` selesai kode; tautan Billing `BE-RWI-159` belum dikerjakan |
| `RWI-DEC-218`, `219` Perkiraan tarif di Pemesanan Ruangan Bedah | `RWI-DEC-218`, `219`, `220` (5) | Backend 12.15 | API 11.1, 11.11 | — (`EXISTING / REUSE` `patient-procedures`) | `FE-RWI-193` | `UAT-RWF-43` (`RWI-AC-338`) | Belum dikerjakan |

## 2. Definition of Done PRD → bukti

| Butir DoD (`04-prd-to-mvp.md` 23.19) | Task | Bukti |
|---|---|---|
| Menu Pemesanan Ruangan Bedah tidak lagi *placeholder* dan setiap pesanan merujuk satu order | `BE-RWI-175`, `FE-RWI-193` | `UAT-RWF-05`, `32`; `AC-RWF-040`, `048` |
| Kasus tidak dapat "Siap" tanpa pra-operasi versi terbaru terkonfirmasi dua akun | `BE-RWI-176`, `FE-RWI-195` | `AC-RWF-042`, `046`, `093`, `094`; `UAT-RWF-21` |
| Serah terima hanya diterima penerima sah di bed unit tujuan | `BE-RWI-177`, `FE-RWI-196` | `AC-RWF-043`, `047`; `UAT-RWF-13` |
| Biaya operasi tepat sekali per komponen, nol untuk kasus batal atau ditolak | `BE-RWI-178`, `BE-RWI-179` | `AC-RWF-044`, `092`, `099`; idempotensi `INV-RWF-29` lewat verifikasi proses bisnis |
| Permintaan admisi tanpa admisi otomatis | `BE-RWI-181`, `FE-RWI-198` | `AC-RWF-080`, `088`, `089`; `UAT-RWF-16`, `33` |
| Status Ditolak di OK dan bangsal | `BE-RWI-174`, `FE-RWI-194`, `FE-RWI-197` | `AC-RWF-085`, `099`; `UAT-RWF-24` |
| Ringkasan operasi baca-saja | `BE-RWI-180`, `FE-RWI-196` | `AC-RWF-081`, `082`; `UAT-RWF-17` |
| Alur OK lama tidak berubah | `BE-RWI-174`, `176`, `177`, `179` | Verifikasi proses bisnis regresi `testing/acceptance-test-matrix.md` bagian 20 |

## 3. Gap dan catatan

| ID | Gap | Penanganan | Pemilik |
|---|---|---|---|
| TRC-RWF-02 | Tiga isian komponen operasi pada formulir master tarif tidak punya layar di kontrak frontend mana pun (API 11.9 menunjuk kontrak `keperawatan`, yang hanya menyebut `MedicalEquipmentId`) | Backend di `BE-RWI-172`; formulir di `keperawatan` `FE-RWI-187` | Muhammad Hamzah |
| — | Nilai `OprPlannedAnesthesiaType` dan isi awal butir persiapan bedah | Gerbang produksi; disahkan pemilik OK dan pemilik klinis | Ikbal Yulianto; pemilik klinis |
| — | Tarif komponen operasi dan tarif bahan OK di master tarif | Prasyarat data, diisi pemilik tarif (`02-backend-architecture.md` 12.12); tanpa itu baris "tarif belum ada" | Yasmina / pemilik tarif |
| — | `E5` dipecah dua (`BE-RWI-174` kolom kasus, `BE-RWI-176` tabel pra-operasi) | Catatan perencanaan; urutan `E4` → tabel pra-operasi dan `E5` → `E6` tetap terjaga lewat dependency | Muhammad Hamzah |
| — | Endpoint tolak order ikut `MVP-1` bersama kolom `E5`, lebih awal dari `MVP-2` di 23.20 | Catatan perencanaan; pemilik dapat menahan rilisnya | Muhammad Hamzah |
| — | Pemberian hak `OperatingRoomHandover : Receive`, `OperatingRoomWardPreOp : Send/Confirm`, `OperatingRoomCase : Reject`, `InpatientAdmissionReferral : Read` | Konfigurasi Akses Role oleh admin hak akses; `Receive` tidak disalin otomatis | Admin hak akses |

Tidak adanya automated test backend **bukan** gap (`rules/backend/TEST_POLICY.md`).
