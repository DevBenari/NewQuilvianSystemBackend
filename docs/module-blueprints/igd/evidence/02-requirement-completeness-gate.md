# IGD — Requirement Completeness Gate (slice encounter-first)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Blueprint revision | `6` (`draft`) |
| Assessment revision | `0.1` — **terbatas pada satu slice** |
| Assessment date | 22 September 2026 (`Asia/Jakarta`) |
| Assessment status | `CURRENT` |
| Readiness slice | **`PARTIALLY_READY`** — tujuh sub-slice `READY_FOR_DOMAIN_DESIGN`, satu sub-slice (kelayakan dokter jaga) `BUSINESS_DECISION_REQUIRED` |
| Readiness modul di luar slice | **Tidak dinilai** — tetap `UNCLASSIFIED` seperti manifest bagian 0 |
| Ready destination | `design-business-module` langsung. `hospital-domain-architect` **tidak dipakai** — alasannya di bagian 10 |
| Business evidence | [`00-interview-decisions.md`](../00-interview-decisions.md) — `IGD-DEC-139`, `141`…`148`, `150`…`154` (`approved`, Rizki Gunawan, 22 September 2026); `IGD-DEC-140` U1 dan `IGD-DEC-149` `superseded` oleh `IGD-DEC-151` |
| Capability evidence | [`01-existing-capability-map.md`](../01-existing-capability-map.md) revisi `3` + **suplemen `3.2`** (impact scan 22 September 2026) |
| Audit evidence | [`2026-09-22-desain-encounter-first.md`](./2026-09-22-desain-encounter-first.md) — `IGD-EV-140`…`144` |
| Backend snapshot | `NewQuilvianSystemBackend` `rizkiG` `0d13f3a8` |
| Frontend snapshot | `QuilvianSystemFrontendDev` `RizkiV2` `c941012ac` |
| Write boundary | Hanya artefak blueprint; nol source, nol migration, nol kueri basis data |

## 1. Scope penilaian

Gate ini menilai **satu** slice proses: perjalanan pasien IGD dari **didaftarkan di loket** sampai
**kunjungannya lahir dan kelak ditutup**, dalam arah encounter-first. Dipecah menjadi delapan
sub-slice supaya satu sub-slice yang belum siap tidak menahan yang lain.

| Sub-slice | Isi | Keputusan |
| --- | --- | --- |
| `S1` | Pendaftaran encounter Emergency dan penjaga episode terbuka (satu pasien satu episode, override beralasan, penguncian serentak, tanpa antrean, pembatasan jalur umum Registrasi) | `IGD-DEC-139`, `144`, `145`, `146`, `153` |
| `S2` | Daftar *Menunggu Triage* terpadu | `IGD-DEC-139` butir 2, `IGD-DEC-144` |
| `S3` | Kelahiran kunjungan: Mulai Triage dan Tangani Segera, termasuk waktu tiba | `IGD-DEC-139` butir 3, `143`, `147`, `152`, `154` |
| `S4` | Pasien pergi sebelum ditriage | `IGD-DEC-142` |
| `S5` | Encounter ikut ditutup saat kunjungan berakhir | `IGD-DEC-139` butir 5, `IGD-DEC-148` (TK-1, TK-2) |
| `S6` | Rekonsiliasi encounter Emergency historis (K1–K4) | `IGD-DEC-148` |
| `S7` | Kelayakan dokter jaga IGD dan override | `IGD-DEC-141` |
| `S8` | Pasien tanpa identitas dengan rekam pengganti | `IGD-DEC-151` |

**Di luar penilaian:** penggabungan rekam pasien (Master Patient), aturan tarif dan deposit
(Billing), aturan antrean rawat jalan (Registrasi), sumber roster final (Human Resource / pemilik
jadwal), serta seluruh area IGD lain (pengkajian, penunjang, obat, kepergian, kewenangan unit).

## 2. Bukti dan wewenang

| Bukti | Wewenang | Perlakuan gate |
| --- | --- | --- |
| Keputusan pemilik 22 September 2026 (tiga pass wawancara) | Product/Domain Owner IGD — wewenang utama untuk **apa yang harus dibangun** | `CONFIRMED` |
| Butir yang menurut `IGD-DEC-150` wajib ditinjau Clinical Governance / Nursing authority | Di-approve Product/Domain Owner, peninjau klinis belum ditunjuk (`IGD-OQ-099`) | `CONFIRMED` dengan catatan peninjauan; **tidak** diperlakukan sebagai kebijakan klinis yang sudah disahkan |
| Asumsi `IGD-ASM-001`, `IGD-ASM-002` | Belum dikonfirmasi peninjau klinis | `PROPOSED` |
| Capability map suplemen 3.2 dan evidence `IGD-EV-140`…`144` | Bukti implementasi V2 terkini — wewenang utama untuk **apa yang ada sekarang** | `CONFIRMED` untuk kondisi as-is |
| Baseline rumah sakit Indonesia | Tidak dipakai pada pass ini | — |

**Pertentangan yang sudah diselesaikan sebelum gate.** `IGD-CONF-06` (jalur U1 tanpa penghasil di
layar), `IGD-CONF-07` (dua jalur NoShow), `IGD-CONF-08` (penimpaan identitas lewat `PUT`) —
diselesaikan `IGD-DEC-151`, `153`, `154`. Tidak ada conflict terbuka di dalam slice.

## 3. Temuan kelengkapan — 18 dimensi

| ID | Dimensi | Temuan | Status bukti | Dampak gap |
| --- | --- | --- | --- | --- |
| 01 | Tujuan | Satu pasien paling banyak satu episode Emergency terbuka; encounter tanpa kunjungan adalah keadaan *Menunggu Triage* yang sah; kunjungan lahir saat proses klinis dimulai; encounter tidak tertinggal terbuka | `CONFIRMED` | Tidak ada |
| 02 | Aktor | Petugas pendaftaran (encounter, override, batal daftar sebelum kunjungan); perawat triage (Mulai Triage, Tangani Segera, NoShow, waktu tiba); admin berwenang (rekonsiliasi); dokter (ditetapkan, bukan penetap) | `CONFIRMED` | Tidak ada. Peran mana yang memegang hak akses adalah konfigurasi (`IGD-UNK-11`) |
| 03 | Pemicu / Prasyarat | Encounter lahir di loket; kunjungan lahir dari encounter yang belum berakhir; rekonsiliasi hanya sesudah penutupan otomatis berjalan (`BE-IGD-051` sebelum `BE-IGD-052`) | `CONFIRMED` | Tidak ada |
| 04 | Alur Utama | Daftar → *Menunggu Triage* → Mulai Triage/Tangani Segera → penanganan → kunjungan berakhir → encounter ikut berakhir | `CONFIRMED` | Tidak ada |
| 05 | Alur Alternatif / Exception | Pendaftaran ganda (tolak/override); dua pendaftaran serentak; pasien pergi sebelum triage (NoShow); salah daftar (batal sebelum kunjungan); Mulai Triage dan Tangani Segera bertabrakan; pasien tanpa identitas; data lama K1–K4 | `CONFIRMED` | Tidak ada di dalam slice |
| 06 | Data Minimum | Kunjungan: data turunan server saja (`IGD-DEC-143`); waktu tiba wajib di Mulai Triage dengan prefill `RegisteredAt` dan penanda konfirmasi (`IGD-DEC-147`); NoShow: pelaku, waktu, alasan; override: encounter baru, episode yang dilangkahi, alasan, pelaku, waktu | `CONFIRMED` | Bentuk fisik penyimpanan adalah wewenang desain |
| 07 | Aturan Bisnis / Validation | Rumus episode terbuka klausa A+B dan lima tanda "berakhir"; larangan heuristik waktu; batas koreksi waktu tiba (`IGD-DEC-152`); `PATCH …/status` menolak Emergency, `…/cancel` hanya sebelum kunjungan; identitas kunjungan terkunci | `CONFIRMED` | Tidak ada |
| 08 | Status / Perubahan Status | Encounter: berakhir lewat `NoShow` (IGD), `Completed`/`Cancelled` (ikut kunjungan), `Cancelled` (salah daftar sebelum kunjungan). Kunjungan: lahir `WaitingForTriage` atau `InTreatment`; status tidak pernah mundur; NoShow final | `CONFIRMED` | Tidak ada |
| 09 | Peran / Authorization | Aksi NoShow ber-hak akses IGD tersendiri; override tanpa permission baru; rekonsiliasi admin khusus; nama resource/aksi diputuskan desain | `CONFIRMED` (aturan) / `PROPOSED` (string permission) | `NON_BLOCKING_STANDARD` — string permission dikunci di kontrak |
| 10 | Dependency Antarmodul | Registrasi (pintu encounter, jalur status/cancel — pemilik belum dipetakan, disentuh di bawah `IGD-DEC-135`); Medical Record (penguncian catatan `RM-DEC-003`); Billing (hanya membaca status); Blood Bank/Medical Record (membaca status berakhir); Master Patient (penggabungan rekam) | `CONFIRMED` | Penggabungan rekam di luar slice — lihat `S8` |
| 11 | Integrasi Internal / Eksternal | Nol integrasi eksternal. Internal lintas modul lewat panggilan service dalam satu proses | `CONFIRMED` | Tidak ada |
| 12 | Hasil Akhir | Nol encounter Emergency tertinggal terbuka sesudah kunjungan berakhir; nol episode ganda tanpa override; daftar triage satu sumber | `CONFIRMED` | Tidak ada |
| 13 | Pembatalan / Koreksi | NoShow final (daftar ulang); batal daftar hanya sebelum kunjungan; koreksi waktu tiba dengan batas; rekonsiliasi dapat dibalik per run | `CONFIRMED` | Tidak ada |
| 14 | Audit / Histori | Pelaku dari token, waktu dari server, alasan wajib untuk NoShow/override/rekonsiliasi; override tambah-saja; penanda konfirmasi waktu tiba | `CONFIRMED` | Tidak ada |
| 15 | Notifikasi | Tidak ada keputusan notifikasi. Kewajiban tindak lanjut pasien berisiko yang pergi (menelepon pasien) disebut sebagai butir peninjauan Clinical Governance (`IGD-DEC-150`) | `PROPOSED` | `NON_BLOCKING_STANDARD` — tidak mengubah struktur data slice ini |
| 16 | Dampak Billing / Charge | NoShow tidak ditagih; pembuatan encounter tidak membuat tagihan; `Cancelled` tidak ditagih (risiko kunjungan batal padahal sempat dilayani dinyatakan) | `CONFIRMED` | Tidak ada |
| 17 | Dampak Keselamatan Klinis | Tangani Segera tanpa ketikan dan triage disusulkan; pasien tanpa identitas tetap dapat diberi seluruh catatan klinis lewat rekam pengganti; override episode ganda tidak pernah menahan penanganan; kriteria menyatakan pasien pergi = alasan wajib (`IGD-ASM-001`) | `CONFIRMED` + `PROPOSED` (`IGD-ASM-001`, `002`) | `NON_BLOCKING_STANDARD` untuk kedua asumsi — approve Product, wajib tinjau klinis (`IGD-DEC-150`). **`BLOCKING` untuk `S7`** — lihat bagian 5 |
| 18 | Pelaporan / Traceability | Angka "pergi sebelum ditriage" dapat dihitung (Emergency + NoShow); laporan pemakaian override sebagai kendali (`IGD-DEC-145`); laporan waktu tiba memisahkan nilai fallback | `CONFIRMED` (kebutuhan data) / `MISSING` (layar laporan) | `NON_BLOCKING_STANDARD` — layar laporan adalah slice kemudian; datanya sudah dijamin |

## 4. Himpunan requirement yang `CONFIRMED`

1. Registrasi membuat encounter Emergency; pendaftaran tidak lagi melahirkan kunjungan (`IGD-DEC-139`).
2. Episode terbuka = klausa A (encounter Emergency belum berakhir, lima tanda) **atau** klausa B (kunjungan belum `Completed`/`Cancelled`); tanpa heuristik waktu (`IGD-DEC-139` + koreksi).
3. Penjaga episode di pintu encounter, dengan kunci per pasien pada setiap jalur pembuka episode (`IGD-DEC-146`).
4. Override pendaftaran ganda: setiap petugas pendaftar, alasan wajib, disimpan di entity IGD tambah-saja (`IGD-DEC-145`).
5. Encounter Emergency tidak membuat antrean, dijamin kode (`IGD-DEC-144`).
6. `PATCH …/status` menolak Emergency; `…/cancel` hanya sebelum kunjungan (`IGD-DEC-153`).
7. Daftar *Menunggu Triage* satu sumber, gabungan encounter dan kunjungan (`IGD-DEC-139`).
8. Mulai Triage → kunjungan `WaitingForTriage`; Tangani Segera → `InTreatment` dalam satu permintaan tanpa ketikan; tabrakan dimenangkan Tangani Segera; status tidak mundur (`IGD-DEC-143`).
9. Waktu tiba diisi perawat triage, prefill `RegisteredAt`, penanda konfirmasi; koreksi tidak boleh sesudah peristiwa klinis pertama maupun di masa depan (`IGD-DEC-147`, `152`).
10. Identitas kunjungan terkunci pada `PUT` (`IGD-DEC-154`).
11. Pasien pergi sebelum triage → `NoShow` oleh perawat triage lewat aksi IGD; final; tidak ditagih (`IGD-DEC-142`).
12. Kunjungan berakhir menutup encounter pada satu penyimpanan; hapus lunak tidak menutup; encounter `Outpatient` tertaut ikut ditutup (`IGD-DEC-139`, `148`).
13. Rekonsiliasi lewat endpoint admin preview + execute; hanya K1; audit per run; dapat dibalik (`IGD-DEC-148`).
14. Pasien tanpa identitas memakai rekam pengganti lewat alur pasien baru yang ada; ditandai `IsUnknownPatient` + alias (`IGD-DEC-151`).
15. Dokter penanggung jawab hanya dari kandidat layak; validasi ulang di backend; override beralasan; tidak pernah buntu (`IGD-DEC-141` — **prinsip**).

## 5. Register gap

### 5.1 `BLOCKING`

| Decision ID | Pertanyaan | Kemampuan terdampak | Bukti saat ini | Usulan baseline | Dampak | Pemilik | Status | Dampak desain |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `IGD-OQ-102` | Dari mana daftar dokter jaga IGD diambil, dan apa kriteria "layak" pada waktu T? | `S7` | `IGD-EV-140`: pilihan hari ini = seluruh master; `MstDoctorSchedule` ber-DNA poliklinik dan belum terbukti berisi jadwal IGD; jembatan identitas cuti tidak ada. Kueri E1–E3 belum berangka | Kandidat K-a/K-b/K-c pada `IGD-DEC-141` — **usulan agent, bukan keputusan** | Model data (sumber roster), kepemilikan master (Human Resource / pemilik jadwal), keselamatan klinis (dokter yang tidak bertugas ditetapkan sebagai penanggung jawab) | Product/Domain Owner IGD + pemilik jadwal dokter / Human Resource (belum dipetakan) | `OPEN` | `S7` berhenti. Sub-slice lain tidak bergantung padanya |
| `IGD-OQ-103` | Di mana penanda "penugasan ini override kelayakan" disimpan — kolom baru pada `EmgDoctorAssignment`, atau teks pada `AssignmentReason`? | `S7` | `IGD-EV-144` butir 4: hanya `AssignmentReason` yang ada | Kolom tersendiri pada tabel IGD (tidak menyentuh tabel global) | Model data yang dipersistensi, audit, pelaporan override | Product/Domain Owner IGD | `OPEN` | Ikut `S7` |

### 5.2 `NON_BLOCKING_STANDARD`

| Butir | Status | Keterangan |
| --- | --- | --- |
| Kriteria menyatakan pasien pergi = alasan wajib, tanpa jumlah panggilan minimum (`IGD-ASM-001`) | `PROPOSED` | Approve Product; wajib tinjau Clinical Governance + Nursing (`IGD-DEC-150`). Menambah jumlah panggilan kelak tidak mengubah struktur NoShow |
| Batas waktu triage susulan sesudah Tangani Segera = SLA triage yang ada (`IGD-ASM-002`) | `PROPOSED` | Sama |
| Kewajiban tindak lanjut pasien berisiko yang pergi | `PROPOSED` | Kebutuhan notifikasi/tugas; slice kemudian |
| Layar laporan pemakaian override dan laporan "pergi sebelum ditriage" | `MISSING` | Data sudah dijamin requirement 4 dan 11; layarnya slice kemudian |
| Label tampilan NoShow khusus IGD | `CONFIRMED` (harus ada) / `PROPOSED` (bunyinya) | Teks label adalah keputusan tampilan — `DEV_DISCRETION` tidak berlaku untuk istilah klinis; bunyinya dikunci pada kontrak frontend |
| String `[AccessPermission]` untuk aksi baru | `PROPOSED` | Dikunci desain di permission matrix |
| Nasib `POST /emergency-visits` untuk pasien tanpa `PatientId` | `PROPOSED` | Tidak lagi punya kebutuhan bisnis baru (`IGD-DEC-151`); dibiarkan untuk data lama atau ditutup — diputuskan desain |

### 5.3 `CONFIGURABLE_DEFAULT`

| Butir | Keterangan |
| --- | --- |
| Peran yang diberi hak akses aksi baru (NoShow, Mulai Triage, rekonsiliasi admin) | Pemberian hak ke peran ada di basis data; berbeda antar rumah sakit (`IGD-UNK-11`) |
| `IsQueueRequired` pada master unit/klinik IGD | Tidak lagi berpengaruh — `IGD-DEC-144` menjamin lewat kode; nilainya boleh dirapikan pemilik master (`IGD-UNK-06`) |

### 5.4 Prasyarat data, bukan keputusan

| Butir | Menahan | Tidak menahan |
| --- | --- | --- |
| Angka kueri D (K1–K4) | **Eksekusi** `S6` di setiap lingkungan | Desain `S6` |
| Angka kueri A/B (`BE-IGD-050`) | Pengecualian ✅ `BE-IGD-050` | Desain slice ini |
| `IGD-UNK-10` (jumlah rekam pengganti yang sudah ada) | — | Desain `S8` |

## 6. Decision Log yang dirujuk

| ID | Status | Catatan |
| --- | --- | --- |
| `IGD-DEC-139`, `141`…`148`, `150`…`154` | `approved` | Dasar seluruh `CONFIRMED` |
| `IGD-DEC-140` (U1), `IGD-DEC-149` | `superseded` | Oleh `IGD-DEC-151` |
| `IGD-OQ-093` | `superseded` sebagian | Realisasi terbuka di `BE-IGD-053` — bukan gap requirement |
| `IGD-OQ-098` | `open` | Penggabungan rekam pasien — dependency luar slice, **tidak** menahan `S8` |
| `IGD-OQ-099` | `open` | Penunjukan Clinical Governance / Nursing — tidak menahan (`IGD-DEC-150`) |
| `IGD-OQ-102`, `IGD-OQ-103` | `open` — **baru dari gate ini** | Menahan `S7` saja |

## 7. Kesiapan per sub-slice

| Sub-slice | Kesiapan | Catatan / dependency |
| --- | --- | --- |
| `S1` Pendaftaran + penjaga episode | `READY_FOR_DOMAIN_DESIGN` | Titik sentuh Registrasi; pemilik belum dipetakan (`IGD-DEC-135`) |
| `S2` Daftar *Menunggu Triage* | `READY_FOR_DOMAIN_DESIGN` | — |
| `S3` Kelahiran kunjungan + waktu tiba | `READY_FOR_DOMAIN_DESIGN` | `IGD-ASM-002` tetap `PROPOSED` |
| `S4` Pergi sebelum triage | `READY_FOR_DOMAIN_DESIGN` | `IGD-ASM-001` tetap `PROPOSED` |
| `S5` Encounter ikut ditutup | `READY_FOR_DOMAIN_DESIGN` | Penguncian catatan `RM-DEC-003` dipakai ulang |
| `S6` Rekonsiliasi historis | `READY_FOR_DOMAIN_DESIGN` | Eksekusi menunggu angka kueri D |
| `S7` Kelayakan dokter jaga | **`BUSINESS_DECISION_REQUIRED`** | `IGD-OQ-102`, `IGD-OQ-103` |
| `S8` Pasien tanpa identitas | `READY_FOR_DOMAIN_DESIGN` | Penggabungan rekam = Master Patient (`IGD-OQ-098`), di luar slice |

**Dependency di antara keduanya.** `S7` tidak menjadi prasyarat `S1`…`S6`/`S8`. Penugasan dokter
yang sudah berjalan (`BE-IGD-045`) tetap dipakai apa adanya sampai `S7` siap.

## 8. Yang boleh berjalan

`S1`…`S6` dan `S8` diserahkan ke `design-business-module`. Handoff wajib:

- mempertahankan rumus episode terbuka (klausa A+B, lima tanda) sebagai **satu** aturan;
- tidak menambah ruas IGD ke tabel global `RegPatientEncounter` (`IGD-DEC-145`);
- memperlakukan `IGD-ASM-001`/`002` sebagai `PROPOSED`, bukan kebijakan klinis;
- tidak merancang penggabungan rekam pasien;
- menandai setiap endpoint yang belum ada di kode sebagai **Rencana (belum tersedia)**.

## 9. Yang harus berhenti

- **`S7`** — desain sumber roster, kriteria kelayakan, dan penyimpanan penanda override, sampai
  `IGD-OQ-102` dan `IGD-OQ-103` dijawab lewat `grill-me` (dan angka E1–E3 tersedia).
- Gate ini tidak membuat entity, schema, endpoint, UI, migration, maupun task implementasi.

## 10. Handoff ke `design-business-module`

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `IGD-BP-001`, revisi `6` |
| Slice | Encounter-first, sub-slice `S1`…`S6`, `S8` |
| Requirement readiness | `PARTIALLY_READY` (slice); `READY_FOR_DOMAIN_DESIGN` untuk sub-slice yang diserahkan |
| `domain_architecture_readiness` | **`DOMAIN_ARCHITECTURE_NOT_RUN`** — slice ini berada di dalam bounded context IGD yang sudah ada; kepemilikan lintas modul yang material (tabel encounter milik Registrasi, entity override milik IGD, penggabungan rekam milik Master Patient, penguncian catatan milik Medical Record) **sudah diputuskan** pemilik (`IGD-DEC-144`, `145`, `151`, `153`); tidak ada master data bersama baru; dampak billing dan keselamatan klinis sudah dinyatakan per keputusan |
| Backend / frontend SHA | `0d13f3a8` / `c941012ac` |
| Blocking decision ID | `IGD-OQ-102`, `IGD-OQ-103` — hanya `S7` |
| Dependency owner | Registrasi (belum dipetakan), Medical Record, Billing (baca saja), Master Patient (belum dipetakan), Clinical Governance / Nursing authority (peninjau, belum ditunjuk) |
| Keluaran hilir yang diharapkan | Kontrak API, state, validation, integration, permission/audit yang diselaraskan; FR/AT kebutuhan baru; flowchart alur encounter-first; kamus data untuk tabel `Baru`/`Diperbarui`; penyelesaian `koreksi_desain_tertunda` manifest |

### Kesimpulan gate

Slice encounter-first `PARTIALLY_READY`. Tujuh dari delapan sub-slice siap untuk desain target.
Kelayakan dokter jaga (`S7`) berhenti di `IGD-OQ-102` dan `IGD-OQ-103`. Seluruh area IGD di luar
slice ini **belum** dinilai, dan manifest bagian 0 tetap mencatatnya sebagai gerbang yang belum
terpenuhi.
