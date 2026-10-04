# Kesiapan `MVP-8` — Penutupan kunjungan lewat disposisi (`EPIC IGD-13`) — 4 Oktober 2026

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` · revision **`8`** · status `draft` (irisan penutupan lewat disposisi `approved` lewat `IGD-DEC-170`, amandemen `IGD-DEC-175` dan `IGD-DEC-186`) |
| Slice | `MVP-8` = `EPIC IGD-13`: `FR-IGD-086`…`095`, `AT-IGD-186`…`197` (`AT-IGD-190` konsekuensi hilir modul Laboratorium, bukan FR IGD); task `BE-IGD-060`…`063`, `FE-IGD-041`, `042`. **Di luar slice** (PRD §9.1): data lama (`IGD-DEC-167`), perubahan modul Bank Darah dan Laboratorium (`IGD-DEC-169`), rekonsiliasi `BE-IGD-052` |
| **Putusan** | **`READY_WITH_CONDITIONS`** — siap masuk UAT untuk alur penutupan **tanpa** serah terima dan sikap pesanan, dengan enam syarat pada bagian 5. **Belum** siap produksi: DoD butir 1 baru terpenuhi sebagian (pemicu serah terima dan sikap pesanan tertahan `BE-IGD-039`) dan UAT belum berjalan |
| Skill | `verify-module-readiness` — hanya membaca source dan bukti; tidak mengerjakan perbaikan |
| Wewenang tulis | Berkas ini dan satu baris status pada `MODULE-STATUS.md`, atas perintah Rizki Gunawan 4 Oktober 2026 (*"lanjut sesuai saran anda jalankan … verify-module-readness MVP-8"*) |
| Backend diperiksa | `NewQuilvianSystemBackend` `rizkiG` **`c1f79f79`** (`ahead 12`); source IGD tanpa perubahan working tree — yang berubah hanya dokumen blueprint 4 Oktober. Build Rizki 3 Oktober 2026: 0 error, 0 warning |
| Frontend diperiksa | `QuilvianSystemFrontendDev` `RizkiV2` **`19ba512de`** (`ahead 3`) + **dua berkas `FE-IGD-042` belum di-commit** (`emergency-assessment-constant.jsx`, `emergency-assessment-status-action.utils.js`); build agent 4 Oktober 11.46 memuat keduanya |
| Kontrak | API **`0.14.0`** §9, validation **`0.11.0`** §6 aturan 2, §11, §11.1, §11.2; state **`0.8.0`** §9; permission/audit **`0.6.0`** §8; integration **`0.5.0`** §6 — `approved` (`IGD-DEC-170`, `175`, `186`) |
| Hash masukan | Kelima kontrak, `02`, `03`, `04`, `erd/data-dictionary.md`, dan `flowcharts/penutupan-lewat-disposisi.md` **identik** dengan manifest 0k (`9b9b29be…`, `37761c90…`, `02d6c649…`, `24234cb2…`, `0f0a544f…`; isi dinormalkan LF). `00-interview-decisions.md` **berbeda** (`d44df2b6…` vs `cac57c06…`) karena `IGD-DEC-188`, `189`, dan `IGD-OQ-113` ditambahkan 4 Oktober — bukan pergeseran kontrak (syarat C6) |
| Keputusan relevan | `IGD-DEC-163`…`177`, `180`, `183`…`189`; asumsi `IGD-ASM-003`; pertanyaan terbuka `IGD-OQ-111`, `112`, `113` (tak satu pun memblokir) |

---

## 1. Putusan dalam bahasa sederhana

Lima dari enam task `MVP-8` selesai, dan yang keenam (`BE-IGD-061`) sudah terbukti untuk semua jalur kecuali dua.
Di lingkungan pengembangan, alur berikut sudah berjalan dan teruji:

- Menandai tindak lanjut *dilaksanakan* menutup kunjungan IGD beserta encounter-nya, bila tidak ada urusan yang
  tertinggal.
- Bila masih ada urusan — observasi yang belum ditutup (termasuk yang dieskalasi), atau kepergian pasien yang belum
  tuntas — kunjungan **menunggu penutupan**. Alasannya terbaca di daftar Pengkajian.
- Kunjungan itu tertutup sendiri saat urusan terakhir dibereskan, atas nama petugas yang membereskannya.
- Pada kunjungan yang sudah berakhir, layar tidak lagi menawarkan aksi observasi yang pasti ditolak.

Yang **belum** terbukti: penutupan lewat **diterimanya serah terima** di unit tujuan dan lewat **sikap atas pesanan**.
Kedua aksi itu sendiri belum dapat dijalankan siapa pun, karena pemeriksaan kewenangan unit (`BE-IGD-039`) selalu menolak.
Selain itu, UAT belum berjalan.

*Contoh syarat yang paling berisiko bila dilewati.* Pasien IGD diputuskan rawat inap; tindak lanjut dilaksanakan dan
kepergiannya ke bangsal tercatat. Perawat bangsal tidak dapat menerima serah terima (`403`), sehingga kunjungan IGD-nya
**terus** menunggu penutupan. Satu-satunya jalan menutupnya hari ini adalah membatalkan kepergian — yang secara klinis
keliru bila pasien memang sudah pindah. Karena itu alur rawat inap tidak boleh masuk UAT sebelum C2 beres.

---

## 2. Cakupan dan denominator

| Yang dihitung | Jumlah | Hasil |
| --- | ---: | --- |
| Functional requirement `FR-IGD-086`…`095` | 10 | 10 terpetakan ke task dan skenario uji (traceability R3.14.2, *"Coverage gap: nihil"*) |
| Skenario uji `AT-IGD-186`…`197` | 12 | 11 FR IGD + `AT-IGD-190` hilir. **Terbukti pada bukti mentah:** 186, 187, 189, 191, 192, 193, 194, 195, 196, 197. **Belum:** 188 (serah terima diterima → tertutup) — tertahan `BE-IGD-039`. `AT-IGD-190` milik modul Laboratorium, tidak diuji di sini |
| Task `MVP-8` | 6 | 5 ✅ (`BE-IGD-060`, `062`, `063`, `FE-IGD-041`, `042`), 1 🟡 (`BE-IGD-061`); 6 laporan tracked ada |
| Butir DoD PRD §9.4 | 11 | 10 terpenuhi, 1 sebagian (butir 1) |
| Grafik dependency R3.14.2 (backend, Mermaid) dan R3.13.1.1 (frontend, Mermaid) | 2 | Panah cocok dengan kolom `Dependency` (5 dan 2); tanda node = tanda kartu = register = traceability untuk keenam task |

---

## 3. Skor per dimensi

**Kemajuan pengerjaan: 5 dari 6 task ✅ = 83%** (`BE-IGD-061` 🟡 dengan 16 dari 19 acceptance terpenuhi, 1 sebagian).
**Kesiapan ujung-ke-ujung (berbobot): ±82%** — rinciannya di bawah. Kedua angka itu sengaja dipisah.

| Dimensi | Bobot | Skor | Bukti | Gap/blocker |
| --- | ---: | ---: | --- | --- |
| Fondasi — keputusan, kontrak, traceability | 15% | 5/6 (83%) | Kontrak `approved` dan hash utuh (manifest 0k); 10/10 FR dan 11/11 AT IGD terpetakan; grafik dan tanda status konsisten di empat tempat | Manifest belum mencatat `IGD-DEC-188`/`189` (hash decision log dan `input_revisions` usang) — C6 |
| Backend | 25% | 3,5/4 (88%) | `…/Services/EmergencyVisitService.cs#TryCloseAfterDispositionAsync(:677),TryCloseAfterBlockerResolvedAsync(:726),SaringMenungguPenutupan(:754),AmbilAlasanMenungguPenutupanAsync(:770)@c1f79f79`; `…/Services/EmergencyDispositionService.cs#ValidateVisitClosureAsync(:108)@c1f79f79` (menghitung `Active` dan `Escalated`); `…/Controllers/EmergencyObservationController.cs#UpdateObservationStatus(:274)@c1f79f79` (aturan 18 di `:290`, aturan 14 di `:294`); `…/Services/EmergencyObservationService.cs#ValidateDetailScopeAsync(:163,:199)@c1f79f79` (aturan 21); `…/Controllers/EmergencyDepartureController.cs#DenganPenutupanSusulanAsync(:275)@c1f79f79` — enam action lewat `ExecuteDeparture` dan `SetOrderAcceptance`; migration `20260930064430` (`ClosedByDispositionId`) | `BE-IGD-061` acceptance 2–3 tidak dapat dibuktikan — C2 |
| Frontend | 20% | 2/2 (100%) | Build agent 11.46 465/465 halaman 0 warning; `eslint` 0 error; uji IGD 91/91; `FE-IGD-041` 4/4 dan `FE-IGD-042` 13/13 pada hasil build, 1440 × 900, akun Perawat IGD | — (dua berkas `FE-IGD-042` belum di-commit — C4) |
| Integrasi dan runtime | 20% | 3/6 (50%) | Dev: migration `BE-IGD-060` diterapkan; backend `c1f79f79` dibuild 0/0 dan berjalan; uji dengan akun peran nyata (Perawat IGD, Petugas Pendaftaran) — log backend 4 Oktober | Serah terima dan pesanan `403` untuk semua pengguna (C2); izin baca Ruang Kerja perawat belum lengkap (C3); lingkungan lain belum dirilis (C5) |
| Verifikasi | 20% | 10,5/12 (88%) | DoD 2–11 terpenuhi pada bukti mentah (bagian 4) | DoD 1 sebagian (C2); UAT belum (C1) |

Skor berbobot: 0,15×83 + 0,25×88 + 0,20×100 + 0,20×50 + 0,20×88 ≈ **82%**.

---

## 4. Definition of Done `EPIC IGD-13` (PRD §9.4)

| No | Butir | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Keempat titik pemicu terpasang dan diuji satu per satu | **Sebagian** | **Terpasang:** keempatnya (source, bagian 3). **Teruji:** tindak lanjut dilaksanakan (`BE-IGD-060` S1, 30 Sep), observasi diselesaikan/dibatalkan (`061-S14`, `S15`, `S19-S1`, `042-U13`, `042-U5`, 4 Okt), kepergian dibatalkan (`061-S19-S11`, 4 Okt). **Belum:** serah terima diterima/ditolak (`061-S2`, `AT-IGD-188`) dan sikap pesanan (`061-S3`, `S12`) — `403` kewenangan unit sebelum sampai ke kode penutupan |
| 2 | Kunjungan tidak pernah tertutup saat masih ada penahan | **Terpenuhi** | `BE-IGD-060` S2 (observasi aktif); `061-S13` (observasi Dieskalasi); `061-S19-S5` (observasi + kepergian, tetap 7 dengan alasan kepergian) |
| 3 | Pelaku penutupan susulan adalah petugas yang membereskan penahan | **Terpenuhi** | `061-S14`, `S19-S1`, `042-U13`: `UpdateBy` = akun Perawat IGD (baris kunjungan dan log backend) |
| 4 | Asal penutupan terbaca, dapat dibedakan dari penutupan manual | **Terpenuhi menurut kontrak** | `ClosedByDispositionId` terisi pada `061-S14`, `S15`, `S19-S1`, `042-U13`; kosong pada penutupan manual (`BE-IGD-060` S3). Kontrak menetapkan pembacaannya lewat kolom audit `EmgVisit` (permission/audit §8.2, `data-dictionary`), **bukan** lewat API — lihat bagian 7 |
| 5 | Pembatalan disposisi atas kunjungan selesai ditolak | **Terpenuhi** | `BE-IGD-062` S1–S5 pada bukti mentah 2 Okt. Jalur pembatalan tidak disentuh pengerjaan ulang 3 Okt (bagian 6) |
| 6 | Saringan menunggu penutupan menampilkan jumlah dan alasan | **Terpenuhi** | `BE-IGD-063` S1, S4–S8 (2 Okt); `061-S13`, `S19-S5` (4 Okt, alasan observasi Dieskalasi dan kepergian); `FE-IGD-041` U2, U6, U7, R pada hasil build (*"23 kunjungan menunggu penutupan"* = `totalData`) |
| 7 | Migration satu kolom diterapkan; `Down()` berpenjaga diuji | **Terpenuhi** | `BE-IGD-060` §5.3: `Down()` berpenjaga diuji agent di basis data terpisah; migration `20260930064430` diterapkan di dev |
| 8 | Nol perubahan `Program.cs`; nol perubahan modul Bank Darah dan Laboratorium | **Terpenuhi** | Commit IGD `16754579`, `74a72399`, `c1f79f79` tidak menyentuh `Program.cs` maupun area `BloodBank`/`Laboratory`; perubahan `Program.cs` pada rentang itu milik merge modul lain |
| 9 | Build 0 error | **Terpenuhi** | Build Rizki 3 Okt atas `c1f79f79` (memuat seluruh source backend `MVP-8`): 0 error, 0 warning. Build frontend 4 Okt 11.46: 0 error, 0 warning |
| 10 | Pada kunjungan berdisposisi dilaksanakan, observasi dapat diselesaikan dan eskalasinya ditolak | **Terpenuhi** | API: `061-S19-S1`, `S7`, `S8`, `S9`. Layar: `042-U3`, `U4`, `U5`, `U8` pada hasil build |
| 11 | Observasi Dieskalasi menahan penutupan di ketiga jalur; aksi observasi pada kunjungan berakhir ditolak dengan pesannya sendiri | **Terpenuhi** | `061-S13` (penutupan susulan), `S16` (selesaikan manual `409`), alasan di daftar (`S13`, `041-U2`); `S14`, `S15`; `S17` (aturan 18), `S18` (aturan 21); layar `042-U9`…`U13`; regresi `S19` dan `042-U1`…`U8` |

---

## 5. Syarat (`READY_WITH_CONDITIONS`)

| Kode | Syarat | Pemilik | Mitigasi / cara memenuhi | Risiko bila dilewati |
| --- | --- | --- | --- | --- |
| **C1** | UAT oleh tim terpisah atas `AT-IGD-186`, `187`, `189`, `191`…`197` — **tanpa** `AT-IGD-188` dan alur sikap pesanan sampai C2 beres | Rizki → tim UAT | Naskah dari PRD §9.3; akun peran nyata (C3); uji pada hasil build, bukan `next dev` | Perawat salah memahami *menunggu penutupan* dan menutup urusan dengan cara keliru (misalnya membatalkan observasi, bukan menyelesaikannya) |
| **C2** | `BE-IGD-039` — jembatan kewenangan unit dan jalan keluar beralasan (`IGD-DEC-092`), lalu uji ulang `061-S2`, `S3`, `S4` kaki kepergian dan pesanan, `S12`, kaki `reject-handover` `S11` | Rizki — menunjuk Security/Privacy owner atau memutuskan sementara (pola `IGD-DEC-174`) | **Sementara:** kunjungan tertahan kepergian dapat ditutup dengan membatalkan kepergian (`061-S19-S11` terbukti); kode penutupan untuk `accept-handover` dan sikap pesanan memakai pembantu yang sama dengan jalur `cancel` yang terbukti (`DenganPenutupanSusulanAsync`); risiko pasien kembali saat kunjungan lama tertahan sudah diterima (`IGD-DEC-180`) | Kunjungan pasien rawat inap/rujukan terus menunggu penutupan; pembatalan kepergian dipakai sebagai jalan pintas yang mengaburkan riwayat |
| **C3** | Konfigurasi peran klinis untuk UAT: izin baca Ruang Kerja perawat (`emergency-triages`, `patient-assessments`, `patient-vital-signs`, `master-data/emergency-disposition-types` — `403` pada uji 4 Okt); putuskan `IGD-OQ-113` (`EmergencyDeparture : Approve`); tetapkan peran yang membuat tindak lanjut (perawat atau dokter) | Rizki / admin peran — **bukan** agen penguji dan bukan SuperAdmin (`IGD-DEC-189`) | Atur lewat Akses Role; buktikan dengan `GET /auth/permissions` tiap akun UAT | Tanpa izin jenis tindak lanjut, pilihan disposisi kosong di layar; riwayat triage dan tanda vital tidak tampil bagi perawat |
| **C4** | Commit dan push: dua berkas `FE-IGD-042` di `RizkiV2`; dokumen blueprint 4 Oktober di `rizkiG` | Rizki | Commit kedua berkas frontend bersama; build 11.46 yang diuji memuat keduanya | Lingkungan bersama menerima layar tanpa tombol nonaktif aturan 18 — perawat menekan *Selesaikan* dan selalu ditolak `409` |
| **C5** | Rilis per lingkungan: migration `20260930064430`, backend `c1f79f79`, frontend dengan `FE-IGD-042`; kueri `IGD-OQ-112` per lingkungan (dev: 4 sebelum uji, 3 sesudah) | Rizki | Daftar periksa rilis; observasi Dieskalasi lama dibiarkan (`IGD-DEC-184`) dan hanya dapat dibatalkan | Kunjungan lama yang punya observasi Dieskalasi sekaligus penahan lain ikut tertahan oleh observasi itu sesudah rilis (validation §11.2 aturan 20) |
| **C6** | Rapikan dokumen: hash decision log dan `input_revisions` manifest (`IGD-DEC-188`/`189`); tabel acceptance laporan `BE-IGD-062` dan `063` yang masih menulis *"Uji belum"*/*"Build belum"* walau tambahan 2 Oktober membuktikannya; paragraf pengantar R3.13.1 frontend (masih API `0.13.0`/validation `0.10.0`/state `0.7.0`); baris snapshot source R3.14 backend | Agent — `manage-module-blueprint` | Hanya baris status, hash, dan tautan bukti; tanpa perubahan kontrak | Pembaca roadmap dan laporan salah membaca keadaan `MVP-8` |

---

## 6. Ketidaksesuaian dokumen dan bukti `STALE`

| Temuan | Letak | Tindakan |
| --- | --- | --- |
| Bukti `BE-IGD-062` S1–S5 (2 Okt) mendahului pengerjaan ulang `c1f79f79` | Laporan `BE-IGD-062` | **Tidak STALE secara perilaku:** pengerjaan ulang hanya mengubah penjaga penutupan, aksi observasi, dan pemantauan; jalur pembatalan disposisi tidak disentuh. Dicakup UAT (C1) |
| Bukti `BE-IGD-063` (2 Okt) mendahului perubahan penjaga | Laporan `BE-IGD-063` | **Dibuktikan ulang 4 Okt** lewat `061-S13`, `S19-S5`, dan `FE-IGD-041` — saringan dan alasan mengikuti penjaga baru |
| Bukti `BE-IGD-060` (30 Sep) mendahului perubahan penjaga | Laporan `BE-IGD-060` | **Dibuktikan ulang 4 Okt:** setiap `RX` pada uji gabungan menjalankan jalur `BE-IGD-060`; penutupan langsung `S19-S1`, `042-U5` |
| Tabel acceptance `BE-IGD-062` dan `063` belum diperbarui sesudah tambahan 2 Oktober | Laporan kedua task | C6 |
| Kartu lama menyebut versi kontrak saat dikerjakan (`BE-IGD-060` validation `0.9.0`, `BE-IGD-062` API `0.13.0`, `BE-IGD-063`/`FE-IGD-041` API `0.12.0`) | Roadmap backend dan frontend | Bukan gap: versi mencatat dasar kerja task. Bagian yang mereka pakai tidak berubah isi pada `0.14.0` |
| Baris `Dependency` beberapa kartu tanpa tanda status | `BE-IGD-063`, `FE-IGD-041`, `FE-IGD-042` | Kosmetik; traceability bertanda lengkap |
| Capability map suplemen dicatat pada SHA lama | `01-existing-capability-map.md` | `STALE` terhadap `c1f79f79`/`19ba512de`; tidak menahan `MVP-8` karena seluruh perilaku slice dibuktikan langsung pada source dan uji |

---

## 7. Risiko yang diterima atau dicatat (bukan syarat)

| Risiko | Dasar | Catatan |
| --- | --- | --- |
| Konfigurasi izin Perawat IGD di dev berasal dari tindakan agen penguji yang disahkan sesudahnya | `IGD-DEC-188` | Bukti 4 Okt diterima dengan penyimpangan tercatat |
| Sandi SuperAdmin dev ada di riwayat Git ter-push sejak Mei 2026 dan pada dua dokumen ter-track | `IGD-DEC-189` | Keputusan penanganan milik lead pemilik |
| Asal penutupan hanya terbaca dari basis data, tidak lewat API atau layar | permission/audit §8.2 | Sesuai kontrak; bila petugas perlu melihatnya di layar, itu kebutuhan baru |
| `arrive` bukan titik pemicu: bila serah terima diterima **sebelum** pasien dicatat tiba, pencatatan tiba sesudahnya tidak menutup kunjungan | Laporan `BE-IGD-061` §3.4 | Belum diputuskan pemilik; kunjungan tetap dapat ditutup manual dan tampil di saringan. Baru relevan sesudah C2 |
| Pesan `403` kewenangan unit menjanjikan *"lanjutkan dengan menyertakan alasan"*, padahal jalannya tidak ada | Kartu `BE-IGD-039` | Bagian dari C2 |
| Disposisi baru masih dapat dibuat pada kunjungan `Completed`; `POST` disposisi langsung berstatus `Executed` tidak memicu penutupan | Laporan `BE-IGD-060`, `062` | Celah lama di luar FR `MVP-8` |
| Tombol tambah pemantauan tidak dinonaktifkan pada kunjungan berakhir (ditolak `409` aturan 21) | `FE-IGD-042`, di luar `IGD-DEC-187` | Pesan tampil lewat jalur galat formulir |
| *Eskalasi* nonaktif pada kunjungan **Selesai** dengan observasi Aktif tidak teruji | `FE-IGD-042` | Data semacam itu tidak dapat dibuat lewat alur normal; teruji pada kunjungan *Dibatalkan* |
| Kolom AKSI daftar Pengkajian terdorong keluar bidang pandang pada nama pasien panjang; pencarian tidak mencakup nama dan RM | Laporan `FE-IGD-041`, `BE-IGD-063` | Masalah lama; keputusan identifikasi pasien |
| Saringan menunggu penutupan memicu hingga ±400 kueri kecil pada `pageSize` 100 | Laporan `BE-IGD-063` | Pantau pada data produksi |
| Cara mencatat pasien yang memburuk sesudah disposisi dilaksanakan | `IGD-OQ-111` | Tidak memblokir `FR-IGD-092`/`093` |
| Kebersihan keamanan di luar slice | — | Tiga `DataProtectionKeys/key-*.xml` ter-track di backend; kredensial teks biasa di skrip uji lokal (`test-with-agy/`, di-gitignore) |

---

## 8. Langkah berikutnya

Satu task terdekat yang langsung dapat dikerjakan: **C6** — penyelarasan dokumen lewat `manage-module-blueprint`
(hash dan `input_revisions` manifest, tabel acceptance `BE-IGD-062`/`063`, paragraf R3.13.1). Pemilik menjalankan
**C4** (commit dan push) dan **C3** (izin peran untuk UAT) sebelum **C1** (UAT). **C2** adalah jalur terpisah yang
menentukan kapan `BE-IGD-061` dapat ✅ dan kapan alur rawat inap/rujukan boleh masuk UAT; **C5** wajib sebelum rilis
ke lingkungan berikutnya.
