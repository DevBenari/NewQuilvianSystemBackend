# Kesiapan Modul IGD — audit sesudah register 120/120 — 29 Oktober 2026

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` · revision **`16`** · status `draft` (irisan yang dipakai `approved`; terbaru `IGD-DEC-368`) |
| **Putusan modul** | **`READY_WITH_CONDITIONS`** — semua task aktif di kedua roadmap ✅. Jalur yang dimiliki IGD, termasuk transfer ke Rawat Inap, terbukti di runtime. **Satu syarat baru (C9) wajib lunas sebelum UAT dimulai:** perubahan kewenangan `IGD-DEC-372` sudah ada di kode, tetapi di luar roadmap, belum diuji runtime untuk jalur triase, dan memakai pola yang dilarang aturan role-access |
| **Putusan `MVP-11`** | **`READY_WITH_CONDITIONS`** — 10 dari 10 task ✅. Acceptance terakhir (`BE-IGD-077` #3–#4) terbukti 28 Oktober 2026 dengan akun admisi bukan SuperAdmin. Syaratnya C5 (scope bed admisi per lingkungan) dan C8 (UAT) |
| Skill | `verify-module-readiness` — membaca source dan bukti, ditambah tiga `GET` runtime; nol perbaikan dikerjakan |
| Wewenang tulis | Berkas ini, satu baris pada `MODULE-STATUS.md`, satu blok decision log — atas perintah Rizki 29 Oktober 2026 (*"lakukan langkah selanjutnya 1 sampai 3 anda yang kerjakan"*) |
| Backend diperiksa | `NewQuilvianSystemBackend` `rizkiG` **`1f4212e8`** + working tree (`EmergencyVisitService.cs` `BE-IGD-042`, dokumen `BE-IGD-077`/`042`); DLL build Rizki 29 Oktober 2026 08.55; backend berjalan di `https://localhost:7184` |
| Frontend diperiksa | `QuilvianSystemFrontendDev` `RizkiV2` **`fc5af984f`**; build produksi **tidak diukur ulang** sejak `HXGe2JMsrS4xsJSmiOFKW` (27 Oktober 2026) |
| Kontrak | API **`0.21.0`** (`3fdeef6d`), validation **`0.21.0`** (`34bc54c1`), state **`0.16.0`** (`0ca795d1`), permission/audit **`0.13.0`** (`1704b5a6`), integration **`0.12.0`** (`654277c8`) — sha256 isi dinormalkan LF, 8 karakter pertama |
| Masukan lain | `00-interview-decisions.md` `4f64c22a`; `backend-roadmap.md` `6d87a8fb`; `frontend-roadmap.md` `8121fa51`; `requirement-traceability.md` `ac0d36c8` (semuanya working tree sebelum berkas ini ditulis) |
| Keputusan relevan | `IGD-DEC-332`, `360`, `361`, `370`–`373`; fakta `IGD-FACT-263`…`266` |
| Pembanding | [Audit 27 Oktober 2026](2026-10-27-kesiapan-modul-igd.md) — `READY_WITH_CONDITIONS`, syarat C1–C8 |
| Aturan pemilik | Tanpa unit test; dilarang kueri basis data; build milik Rizki; UAT oleh tim terpisah dan **tidak** diklaim di sini |

---

## 1. Putusan dalam bahasa sederhana

Dibanding 27 Oktober, tiga syarat yang dulu menunggu modul lain sudah lunas:

- **Rawat Inap:** sudah memasang tombol *Tolak* dan penutupan kunjungan IGD sesudah admit. Keduanya terbukti 28 Oktober 2026.
- **Laboratorium/Radiologi:** aturan *dokter pemberi instruksi* sudah berlaku untuk pesanan dari IGD.
- **Gizi:** menyatakan tidak melayani pasien selama masih di IGD, jadi task itu dibatalkan.

Pemeriksaan jenis encounter yang terakhir (`BE-IGD-042`) juga sudah selesai.

*Contoh yang kini berjalan:* pasien diantar ke bangsal **sebelum** admisi diproses. Begitu petugas admisi menyelesaikan admit,
kunjungan IGD-nya langsung tertutup dalam penyimpanan yang sama. Sebelumnya kunjungan itu menggantung di IGD.

Yang menahan modul ini dari `READY` ada dua hal:

1. **Perubahan kewenangan dokter dan perawat IGD dari 28 Oktober (`IGD-DEC-372`).**
   - Kodenya sudah di-commit, tetapi tidak punya kartu di roadmap.
   - Buktinya baru build dan lint. Penyaring daftar pasien dokter terbukti hari ini, tetapi penolakan triase bagi perawat unit lain belum pernah diuji.
   - Siapa yang dianggap "staf IGD" ditebak dari **nama** departemen atau jabatan yang mengandung "IGD" atau "Gawat Darurat". Bila nama department perawat IGD sungguhan berbeda, ia bisa tertolak saat menyimpan triase. Bagi IGD, itu berarti pasien tidak dapat ditriase.
2. **Syarat lama yang tidak berubah:** keamanan/privasi (C4), kesiapan lingkungan rilis (C5–C7), dan UAT (C8).

## 2. Cakupan dan denominator

| Lapisan | Denominator | Selesai | Bukti |
| --- | --- | --- | --- |
| Task backend | 64 aktif (67 baris − 3 dibatalkan: `BE-IGD-017`, `073`, `075`) | **64 ✅ (100%)** | Register `backend-roadmap.md@6d87a8fb`, dihitung ulang dari berkas; 64 dari 64 bertautan ke laporan yang berkasnya ada |
| Task frontend | 56 aktif (59 baris − 3 dibatalkan: `FE-IGD-010`, `053`, `062`) | **56 ✅ (100%)** | Register `frontend-roadmap.md@8121fa51`; 56 dari 56 punya berkas laporan; 10 di antaranya menautkan laporan di kolom status, bukan kolom terakhir |
| Gabungan | 120 aktif | **120 (100%)** | Atas 126 baris: 120 (95,2%) |
| **Di luar register** | — | **2 task berlaporan tanpa kartu:** `BE-IGD-086`, `FE-IGD-072` | `task/report/backend/BE-IGD-086.md`, `task/report/frontend/FE-IGD-072.md`; nol sebutan di roadmap, traceability, dan `MODULE-STATUS.md` |
| Keputusan terbuka | — | **6**: `IGD-OQ-127`, `129`, `137`, `140`, `142`, `143` (27 Oktober: 9) | Tabel *Blockers and owners* `MODULE-STATUS.md`; `117`, `119`, `132`, `145` kini dijawab atau ditutup |
| Requirement `FR-IGD-*` | 82 baris traceability | Baris yang dinilai ulang: `FR-IGD-102`, `119`, `120` kini *Terbukti/Lunas*; `109`–`111` dan `126` yang dulu `STALE` sudah diselaraskan | `requirement-traceability.md@ac0d36c8`. **Keterbatasan** yang sama dengan 27 Oktober: 52 baris tabel lama tidak dibaca satu per satu |

## 3. Skor per dimensi

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | --- | --- | --- |
| Fondasi — keputusan, desain, kontrak | 15 | Kontrak lima berkas berversi. **6** keputusan terbuka (dari 9). `IGD-DEC-100`…`102` masih `draft` | `00-interview-decisions.md@4f64c22a` | Security/Privacy owner dan Clinical Governance masih `OPEN`. `IGD-DEC-372` `approved` tanpa task |
| Backend | 20 | **64/64** ✅ | `backend-roadmap.md@6d87a8fb`; laporan `task/report/backend/` (67 berkas) | `BE-IGD-086` di luar register (C9) |
| Frontend | 20 | **56/56** ✅ | `frontend-roadmap.md@8121fa51`; laporan `task/report/frontend/` (55 berkas) | `FE-IGD-072` di luar register (C9); build produksi belum diukur ulang sejak 27 Oktober |
| Integrasi dan runtime | 20 | Transfer IGD → Rawat Inap terbukti dua arah (11 langkah `PASS`, `IGD-FACT-264`); Lab/Rad dokter pemberi instruksi terbukti (`IGD-FACT-263`). **2** ketergantungan lintas modul tersisa | `test-with-agy/igd/uji-putaran-b7-bagian2-20261028/*-hasil.json`; `uji-be-igd-042-20261029/2026-10-29T01-59-51-638Z-pasca-hasil.json` | Billing (`IGD-OQ-129`, `FR-IGD-118`); prioritas antrean dan diskon member (`FR-IGD-124`, `125`) |
| Keamanan dan privasi | 10 | **Terbatas** — kewenangan unit (`BE-IGD-039`) dan penyamaran nominal (`FE-IGD-071`) tetap. Penyaring dokter `IGD-DEC-372`(b) terbukti `GET` hari ini | Bagian 4 | Pola kewenangan berbasis nama dan role di `BE-IGD-086` (C9); `IGD-DEC-092` dan `IGD-OQ-143` tetap (C4) |
| Data dan konfigurasi | 5 | **Belum siap untuk lingkungan baru** | `IGD-FACT-236`, `238`, `136`, `265`, `266` | C5, C6; kueri prasyarat `BE-IGD-042` perlu diulang di lingkungan rilis |
| Verifikasi | 10 | Bukti mentah untuk 120 task; **0** test otomatis; UAT belum ada hasil | `IGD-DEC-332`; aturan pemilik | `BE-IGD-086`/`FE-IGD-072` hanya build dan lint |

## 4. Pemeriksaan runtime yang dijalankan audit ini

Tiga permintaan `GET` ke `emergency-visits?pageSize=100`, nol penulisan:

| Akun | Total kunjungan | Sebaran DPJP aktif pada hasil |
| --- | ---: | --- |
| Perawat uji (SuperAdmin) | 255 | 100 baris diambil: 74 tanpa DPJP, dr. Jenah Ali 19, dr. B4 IGD Utama 6, dr. Rendy Pangalila 1 |
| Dokter uji 1 (bukan SuperAdmin) | **20** | dr. Jenah Ali 20 — seluruhnya miliknya |
| Dokter uji 2 (bukan SuperAdmin) | **6** | dr. B4 IGD Utama 6 — seluruhnya miliknya |

Artinya `IGD-DEC-372` butir (b) bekerja di backend. Konsekuensi yang sudah diterima keputusan itu terlihat nyata: kunjungan
tanpa DPJP aktif tidak tampil bagi dokter mana pun.

Butir (d), penolakan `403` bagi perawat unit lain saat menyimpan triase dan observasi, **tidak diuji**. Mengujinya berarti
mencoba menulis triase, dan bila penjaganya ternyata tidak menolak, data triase sungguhan akan tercipta. Itu di luar sifat
audit yang hanya membaca.

## 5. Syarat (`READY_WITH_CONDITIONS`)

| # | Syarat | Keadaan 29 Okt | Pemilik | Mitigasi sementara | Dasar |
| --- | --- | --- | --- | --- | --- |
| ~~C1~~ | Rawat Inap memanggil penutupan susulan dan menyediakan *Tolak* | **Lunas 28 Oktober** | — | — | `BE-IGD-077` ✅, `IGD-FACT-264` |
| ~~C2~~ | Dokter pemberi instruksi Lab/Rad untuk encounter IGD | **Lunas 28 Oktober** | — | — | `BE-IGD-071`, `FE-IGD-057`, `058` ✅; `IGD-DEC-371` |
| ~~C3~~ | Gizi melayani atau tidak pasien IGD | **Lunas 27 Oktober — dijawab *tidak*** | — | — | `IGD-DEC-361`, `FE-IGD-053` dibatalkan |
| C4 | Security/Privacy owner ditunjuk; putuskan `IGD-DEC-092` dan `IGD-OQ-143` | Tetap | Rizki → manajemen | Layar IGD menyaring per kunjungan | `BE-IGD-039`, `IGD-OQ-143` |
| C5 | Lingkungan rilis siap: master data IGD, `IGD-UNK-18`, `max_locks_per_transaction` > 64, scope bed admisi (`BedManagement:UnitAssignments` per `UserId`) diinjeksi per lingkungan | Tetap. Di dev, scope admisi B4 hanya ada di user-secrets PC kantor (`IGD-FACT-265`). Kueri prasyarat `BE-IGD-042` perlu diulang (`IGD-FACT-266`) | Rizki / Platform; scope admisi: Rawat Inap (Release Gate R3.18.5) | Konfigurasi manual per lingkungan | `IGD-FACT-236`, `136`, `265`, `266` |
| C6 | Keputusan atas validasi Emergency Visit yang dimatikan sejak 7 Agustus 2026 | Tetap | Rizki | Sah menurut kontrak | `IGD-FACT-238` |
| C7 | Izin uji tidak terbawa ke lingkungan lain | Bertambah: `LabOrder : Verify`/`RadOrder : Verify` untuk Medis / Dokter Umum dan jadwal satu hari (`IGD-FACT-263`); izin *Tolak* Petugas Pendaftaran (`IGD-FACT-265`, dicabut sesudah audit ini atas perintah Rizki) | Rizki | Cadangan policy tersimpan | `IGD-DEC-358`, `IGD-FACT-263`, `265` |
| C8 | UAT oleh tim terpisah | Tetap — belum ada hasil | Tim UAT | Bukti agen per task | Aturan pemilik |
| **C9 (baru, wajib sebelum UAT)** | `IGD-DEC-372` dibawa ke jalur delivery yang sah: (a) kartu `BE-IGD-086` dan `FE-IGD-072` masuk register, grafik, dan traceability; (b) penentu "staf IGD" diganti dari kecocokan nama departemen/jabatan (`ILike "%IGD%"`, `"%Gawat Darurat%"`) serta `IsInRole("SuperAdmin")`/`UserType` ke mekanisme yang diizinkan, yaitu izin role-access atau penugasan unit `EmergencyUnitAuthorityService` yang sudah ada; (c) 18 baris komentar `//` baru dibersihkan sesuai aturan nol komentar; (d) uji runtime butir (d) dengan akun perawat IGD **bukan SuperAdmin** (harus berhasil) dan perawat unit lain (harus `403`) | **Baru** | Rizki (pemilik IGD dan penulis commit `00cbca33`, `6be6e4774`) | Sampai lunas, uji triase hanya dengan akun yang department atau jabatannya memuat kata IGD/Gawat Darurat | Bagian 6 butir 1–3 |

## 6. Ketidaksesuaian dokumen dan bukti

| # | Temuan | Arah |
| ---: | --- | --- |
| 1 | `BE-IGD-086` dan `FE-IGD-072` punya laporan ✅ tetapi tidak punya kartu, baris register, node grafik, atau baris traceability. Laporan menautkan roadmap yang tidak memuatnya | `plan-module-delivery` — kartukan susulan, seperti `FE-IGD-071` pada 26 Oktober |
| 2 | `EmergencyUnitAuthorityService.PeriksaWewenangKlinisIgdAsync` (`00cbca33`) menentukan kewenangan klinis dari **nama** departemen/jabatan. `IsSuperAdmin` membaca `IsInRole`, klaim role, dan `UserType`. Keduanya dilarang `rules/backend/role-access-rules.md` (hardcode role access). Akibat samping: jabatan dokter yang memuat "IGD" ikut lolos penjaga triase, padahal `IGD-DEC-372`(d) menyebut *hanya Perawat IGD* | Satu task builder backend sesudah butir 1 |
| 3 | 18 baris komentar `//` dan beberapa doc-comment `///` baru di `00cbca33` | Ikut task butir 2 |
| 4 | Node grafik `BE-IGD-044` tanpa tanda, `BE-IGD-045` ganda (satu bertanda, satu tidak) — 27 Oktober: 9 node backend dan 16 node frontend berbeda | `plan-module-delivery` — sisa kecil |
| 5 | Doc-comment `ApplyEncounterClosureAsync` masih menyebut *"`Outpatient` masa transisi"*; method privat `ValidateRequestAsync` di `EmergencyVisitController` tanpa pemanggil | Dicatat di laporan `BE-IGD-042`; bukan syarat |
| 6 | Snapshot EF lintas merge — tidak diukur ulang (sama dengan 27 Oktober) | Diperiksa sebelum merge balik ke integration |

Temuan 27 Oktober yang **sudah beres**: `FR-IGD-109`…`111` dan `126` diselaraskan; fase `MVP-5` dan `MVP-9` `DONE`; tautan laporan `FE-IGD-056` dan `065` ada.

## 7. Risiko yang dicatat (bukan syarat)

| Risiko | Bukti | Catatan |
| --- | --- | --- |
| Kunjungan tanpa DPJP aktif tak terlihat dokter mana pun | Bagian 4 (74 dari 100 baris sampel) | Konsekuensi yang diterima `IGD-DEC-372`; bergantung pada ketepatan perawat menetapkan DPJP |
| Tiga catatan Rawat Inap: izin tombol *Tolak* tidak sama dengan backend, *Tolak* tanpa scope unit, deadlock klik serentak | [Laporan uji B7 bagian 2](../testing/2026-10-28-laporan-uji-putaran-b7-bagian2.md) bagian 1 | Milik Rawat Inap (`IGD-DEC-370`) |
| Regresi tanpa test otomatis | Aturan pemilik | Sama dengan 27 Oktober |
| Risiko 27 Oktober lainnya (`IGD-FACT-251`, `252`, `IGD-OQ-140`, `142`) | Audit 27 Oktober bagian 7 | Tidak berubah |

## 8. Langkah berikutnya

1. **C9 sebelum UAT:**
   - `plan-module-delivery` mengkartukan `BE-IGD-086`/`FE-IGD-072` beserta acceptance-nya.
   - Satu task builder backend mengganti penentu kewenangan berbasis nama dan membersihkan komentar.
   - Uji triase dilakukan dengan dua akun perawat bukan SuperAdmin, satu dari IGD dan satu dari unit lain.
2. Serahkan ke UAT (C8) dengan daftar bukti per task. Putusan dapat naik ke `READY` untuk UAT penuh bila C9 lunas.
3. Kesiapan produksi menunggu C4–C7.
