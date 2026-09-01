# Roadmap Delivery Frontend — Modul Rawat Jalan Billing

> ## `DOWNSTREAM — NOT PART OF DOCTOR DEFINITION OF DONE`
>
> Ketujuh task `RJ-BIL-FE-*` di dokumen ini adalah layar **Billing**: folio, batas klinis–finansial,
> pembagian penanggung, tindakan finansial, rekonsiliasi, dan klaim manual. Tidak satu pun menjadi
> Definition of Done developer Dokter / Rawat Jalan.
>
> | | |
> |---|---|
> | **Owner** | Billing / Revenue Cycle bersama Frontend authority |
> | **Consumer dari** | Clinical fact yang diterbitkan Doctor / Clinical |
> | **Blocks Doctor DoD** | `NO` untuk seluruh task, tanpa kecuali |
>
> Layar workspace dokter — antrean, SOAP/CPPT, resep, tindakan, surat keterangan, dan tombol
> `Selesai Konsultasi` — **bukan** cakupan dokumen ini. Roadmap-nya ada pada
> [doctor-consultation-roadmap.md](doctor-consultation-roadmap.md).
>
> Progress `2 dari 7` adalah angka Billing dan **tidak boleh** dipakai sebagai progress Dokter.

> ## `HISTORICAL SNAPSHOT — DO NOT USE AS CURRENT STATUS`
>
> Metadata, progress, dan bukti test di bawah adalah potret per `2026-08-28` dan **tidak**
> diverifikasi ulang terhadap frontend `HEAD` `baca9650848ded164538ab85405190fafe8785a3`.
>
> | Pernyataan snapshot | Keadaan pada `HEAD` |
> |---|---|
> | `source_commits.frontend: ab4bd836…` | `HEAD` adalah `baca965`. Selisihnya belum dihitung; menyatakan snapshot mana yang menjadi bukti resmi adalah wewenang pemilik frontend |
> | *"`88` test unit lulus; `29` milik `FE-001` dan `21` milik `FE-002`"* | Berkas test yang dirujuk **tidak ditemukan**. `tests/unit` memuat `22` berkas, tidak satu pun bernama billing/folio/clinical-boundary |
> | `RJ-BIL-FE-001` dan `FE-002` `✅ SELESAI` | **Layarnya ada** — `src/app/health-services/billing-management/billing-folio/[encounterId]` dan `.../clinical-boundary/[encounterId]`. Yang hilang adalah bukti test-nya, kemungkinan besar saat integrasi. Menilai ulang adalah wewenang pemilik Billing |
>
> Rincian pada [doctor-consultation-roadmap.md](doctor-consultation-roadmap.md) bagian `2`.

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
module_name: Dokter / Rawat Jalan Billing
snapshot_kind: HISTORICAL_SNAPSHOT
snapshot_observed_at: "2026-08-28"
module_slug: rawat-jalan
module_prefix: RJ-BIL
repository: V2QuilvianSystemFrontendDev
roadmap_revision: 1
input_revisions:
  blueprint-manifest.md: 19
  00-interview-decisions.md: 14
status: APPROVED_FOR_EXECUTION
approval_gate: OWNER_APPROVED
scope: Core internal/manual
owners:
  - "Product/Domain: Sukma Giri"
  - "Frontend authority: OPEN — sesuai 03-frontend-architecture.md bagian 7"
  - "Security/Privacy: OPEN"
approved_by:
  - "User-provided approval authority — RJ-BIL-FE-001 s.d. RJ-BIL-FE-007 pada 2026-08-21"
approved_at: "2026-08-21"
backend_prerequisite: "RJ-BIL-BE-001 — contract backend terkunci dan tersedia untuk consumer"
source_commits:
  frontend: "ab4bd836e05c72d0679e02899258f3773f3869a2"
implementation_authority: GRANTED
implementation_authority_decision: RJ-BIL-DEC-013
builder_execution:
  authorized: [RJ-BIL-FE-001, RJ-BIL-FE-002, RJ-BIL-FE-004, RJ-BIL-FE-005]
  not_authorized: [RJ-BIL-FE-003, RJ-BIL-FE-006, RJ-BIL-FE-007]
  scope_exclusion: "RJ-BIL-FE-002 bagian Radiologi tetap di luar wewenang selama RJ-BIL-BE-004 terblokir"
  executed: [RJ-BIL-FE-001, RJ-BIL-FE-002]
external_adapter: "RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE"
task_count: 7
progress: "2 dari 7 task frontend selesai per 2026-08-28; RJ-BIL-FE-002 selesai untuk Resep/Tindakan/Lab, bagian Radiologi tetap terblokir"
test_evidence: "88 test unit lulus, 0 gagal; 29 milik RJ-BIL-FE-001 dan 21 milik RJ-BIL-FE-002. Build next lulus exit 0. Component render test belum mungkin pada harness node --test."
source_snapshot_drift: "SHA frontend pada manifest (ab4bd836) tertinggal 11 commit dari HEAD (bd31dc99). Sengaja tidak disamakan; wewenang pemilik frontend."
```

---

## 0. Peringatan yang tidak boleh dilewati

> **Roadmap ini berstatus `APPROVED_FOR_EXECUTION` sejak 2026-08-21.** Ketujuh task
> `RJ-BIL-FE-001` s.d. `RJ-BIL-FE-007` sudah disetujui pemilik pekerjaan.
>
> **`RJ-BIL-DEC-013` pada 2026-08-28 menaikkan `IMPLEMENTATION_AUTHORITY` menjadi `GRANTED`.**
> `BUILDER_EXECUTION` menjadi `AUTHORIZED` untuk **empat** task saja — `RJ-BIL-FE-001`,
> `RJ-BIL-FE-002` bagian Lab, `RJ-BIL-FE-004`, dan `RJ-BIL-FE-005` — yaitu yang backend
> pasangannya sudah selesai dan terbukti lulus test. `RJ-BIL-FE-003`, `RJ-BIL-FE-006`, dan
> `RJ-BIL-FE-007` tetap `NOT_AUTHORIZED` karena endpoint-nya belum ada sama sekali.
>
> Wewenang itu mencakup penulisan code dan test frontend. Ia **tidak** mencakup commit, push,
> merge, deployment, maupun perubahan backend. `FRONTEND_AUTHORITY` atas route, menu, dan bentuk
> komponen pada `03-frontend-architecture.md` bagian `7` tetap `OPEN`, begitu pula
> `SECURITY_PRIVACY_SIGNOFF`.
>
> **Tidak satu pun task di bawah boleh mengaktifkan `RJ-BIL-DEP-009`.** Adapter payer eksternal
> tidak boleh punya tombol aktivasi produksi di layar mana pun.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya | Syarat |
| :---: | --- | --- |
| ✅ | **SELESAI** | Layar sudah dibuat, build lulus, dan test acceptance-nya lulus dengan bukti tercatat |
| 🟡 | **KODE SIAP, BELUM DI-BUILD** | `.jsx` sudah ditulis, tetapi build dan test belum dijalankan — sehingga belum ada bukti apa pun bahwa layar itu berjalan |
| ⛔ | **TERBLOKIR** | Endpoint backend pasangannya belum ada, atau task frontend pendahulunya belum selesai |
| tanpa tanda | **Belum dikerjakan** | Endpoint backend pasangannya sudah ada dan wewenang tulisnya sudah diberikan `RJ-BIL-DEC-013`; layarnya yang belum dibuat |

**Keadaan hari ini, 28 Agustus 2026.**

| Hal | Keadaannya |
| --- | --- |
| Task frontend selesai | **2 dari 7** — `RJ-BIL-FE-001` dan `RJ-BIL-FE-002` ✅ selesai `2026-08-28`. Bagian Radiologi `FE-002` tetap ⛔ |
| Task yang backend-nya sudah siap dan belum dikerjakan | **2** — `RJ-BIL-FE-004` dan `RJ-BIL-FE-005`; keduanya kini terbuka karena gerbangnya `FE-002` sudah selesai |
| Task yang backend-nya masih terblokir | **3** — `RJ-BIL-FE-003`, `RJ-BIL-FE-006`, `RJ-BIL-FE-007` |
| Wewenang tulis frontend | **`GRANTED` sejak `RJ-BIL-DEC-013`**, terbatas pada 4 task yang backend-nya siap. 3 task sisanya tetap `NOT_AUTHORIZED` |

Keadaan frontend hari ini, dari capability map pada commit `ab4bd83`:

| Hal | Keadaannya | Bukti |
| --- | --- | --- |
| Workspace Dokter/Rawat Jalan | **Ada dan dapat dijangkau.** Route, antrean, SOAP, CPPT, resep, tindakan, surat keterangan, serta penanganan loading dan error sudah tersedia | `RJ-BIL-CAP-002` — *Ready to reuse* |
| Tab resep dan tindakan | **Ada.** Draft, autosave, dan finalize sudah terintegrasi | `RJ-BIL-CAP-018` — *Ready to reuse*. **Tidak boleh** dijadikan financial source of truth |
| Tab order Lab dan Radiologi | **Lab kini terbaca** lewat `RJ-BIL-FE-002`, sebagai status pada layar Billing — **bukan** sebagai journey order operasional, yang masih tidak ada. Radiologi tetap nihil | `RJ-BIL-CAP-019` — *Missing* pada snapshot audit; `RJ-BIL-CONFLICT-003` masih berlaku |
| Layar billing, folio, payer split, correction status | **Sebagian tertutup `2026-08-28`.** Layar folio baca sudah ada lewat `RJ-BIL-FE-001`. Payer split dan correction status **masih tidak punya consumer** | `RJ-BIL-CAP-020` — *Missing* pada snapshot audit; folio kini terpenuhi |
| Test frontend yang relevan | **Mulai ada.** 29 test unit `RJ-BIL-FE-001` pada `tests/unit/billing-folio-utils.test.mjs`. **Render test masih nihil** — harness `node --test` tanpa `@testing-library` | `RJ-BIL-CAP-021` — *Missing* pada snapshot audit; ditutup bertahap oleh `RJ-BIL-FE-007` |

> **`RJ-BIL-CONFLICT-003` perlu diingat sebelum estimasi dibuat.** Jawaban closure menyatakan Lab
> dan Radiologi sudah tersedia di menu dokter. Source frontend hanya membuktikan resep dan
> tindakan sebagai tab order. Lab dan Radiologi muncul sebagai label CPPT dan metadata master —
> **bukan** alur pemesanan yang berjalan. Siapa pun yang merencanakan pekerjaan berdasarkan
> jawaban closure itu akan salah menghitung.

---

## 1. Batas kewenangan dokumen ini

`03-frontend-architecture.md` menetapkan **kontrak perilaku**: kemampuan apa yang dibutuhkan,
siapa boleh melakukan apa, data dan status apa yang dikonsumsi, dan bagaimana keadaan gagal
ditangani. Ia **tidak** menetapkan route final, lokasi menu, susunan sidebar, bentuk modal atau
drawer, warna status, maupun pustaka komponen.

Urutan wewenang yang berlaku pada setiap task di bawah:

```text
keamanan / privasi / invariant finansial
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

Empat hal yang **bukan** `DEV_DISCRETION`, dan karena itu ditulis sebagai acceptance criteria yang
mengikat:

| Yang mengikat | Isinya | Rujukan |
| --- | --- | --- |
| **Perbedaan enam jenis status** | Frontend wajib membedakan clinical order/fulfillment, milestone processing, charge calculation, allocation, payment/claim settlement, dan financial action approval. Keenamnya tidak boleh dilebur menjadi satu label | Arsitektur bagian 2 |
| **Penanganan keadaan gagal** | `401`/`403` ditampilkan sebagai akses tidak tersedia, **tidak boleh** disembunyikan sebagai kosong. `404` berarti tidak ditemukan, bukan kosong. `409` menampilkan konflik dan tombol reload terkontrol. Timeout **mempertahankan** request identity. Response versi lama **tidak boleh** menimpa state yang lebih baru | Arsitektur bagian 3 |
| **Aksi per peran** | Maker tidak boleh menyetujui permintaannya sendiri; Pharmacy hanya membaca projection; clinical user tidak boleh void/refund/waiver | Arsitektur bagian 4 |
| **Privasi** | Data klinis sensitif tidak masuk custom logger dan hanya tampil sesuai permission. UUID **tidak boleh** menjadi satu-satunya informasi di layar | Arsitektur bagian 6 |

Dua kalimat yang wajib muncul apa adanya di layar:

| Status | Cara menampilkannya |
| --- | --- |
| `OutcomeUnknown` | **"hasil pemrosesan belum dapat dipastikan"** — bukan gagal, dan bukan berhasil |
| `PendingFinancialReview` | **"menunggu tinjauan finansial"** |

Layar fungsional **boleh digabung** selama seluruh kemampuannya tercapai. Karena itu task di bawah
diberi nama menurut **kemampuan**, bukan menurut jumlah halaman.

---

## 2. Aturan paralel dengan backend

Frontend boleh mendahului backend **hanya** setelah kontrak backend yang bersangkutan disetujui,
diberi versi, dikunci hash-nya, dan tersedia sebagai consumer fixture. "Sudah ada di dokumen
kontrak" **belum** cukup; yang dihitung adalah endpoint pasangannya benar-benar dapat dipanggil.

| Task frontend | Backend pasangannya | Keadaan backend per `2026-08-27` |
| --- | --- | --- |
| `RJ-BIL-FE-001` | `RJ-BIL-BE-001` + `RJ-BIL-BE-007` | ✅ **Selesai.** Layarnya sudah dikerjakan dan ✅ selesai `2026-08-28` |
| `RJ-BIL-FE-002` | `RJ-BIL-BE-002`, `BE-003`, `BE-004` | ✅ Resep, tindakan, dan Lab selesai; ⛔ **Radiologi terblokir**. Bagian Radiologi menyusul |
| `RJ-BIL-FE-003` | `RJ-BIL-BE-005` | ⛔ **Terblokir** — menunggu keputusan `RJ-BIL-CONFLICT-001` |
| `RJ-BIL-FE-004` | `RJ-BIL-BE-006` | ✅ **Selesai `2026-08-27`** — wewenang tulis sudah diberikan `RJ-BIL-DEC-013`; layarnya belum dikerjakan |
| `RJ-BIL-FE-005` | `RJ-BIL-BE-007` | ✅ **Selesai** — wewenang tulis sudah diberikan `RJ-BIL-DEC-013`; layarnya belum dikerjakan |
| `RJ-BIL-FE-006` | `RJ-BIL-BE-008` | ⛔ **Terblokir** — menunggu `RJ-BIL-BE-005` |
| `RJ-BIL-FE-007` | `RJ-BIL-FE-001` s.d. `FE-006` | ⛔ Paling akhir |

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Task |
| --- | --- | --- |
| **F0 — Tagihan satu kunjungan dapat dibaca** | Petugas dapat membuka folio sebuah kunjungan dan melihat rinciannya tanpa satu pun angka dapat diubah dari layar | `RJ-BIL-FE-001` |
| **F1 — Batas klinis dan finansial terlihat di layar** | Pesanan obat tidak pernah tampil sebagai lunas; sumber dan versi statusnya terbaca | `RJ-BIL-FE-002` |
| **F2 — Kegagalan dan pemulihan terlihat** | Timeout tidak memancing kirim ulang membabi buta; komponen yang gagal tidak tampil sebagai nol | `RJ-BIL-FE-005` |
| **F3 — Pembagian penanggung terlihat** | Total alokasi ditambah tanggungan pasien selalu sama dengan tagihan bersih; versi lama tetap dapat dilihat | ⛔ `RJ-BIL-FE-003` |
| **F4 — Tindakan finansial dan persetujuannya** | Permintaan yang menunggu persetujuan tidak mengubah angka; self-approval tampil sebagai galat | `RJ-BIL-FE-004` |
| **F5 — Klaim manual per penanggung** | Klaim disetujui tetap `PaymentPending`; adapter eksternal tidak punya tombol aktivasi | ⛔ `RJ-BIL-FE-006` |
| **F6 — Kesiapan sebelum sign-off** | Setiap acceptance criteria UI kritis punya bukti test atau pemilik gap-nya | ⛔ `RJ-BIL-FE-007` |

### Urutan dependency

```text
RJ-BIL-FE-001 (consumer folio + milestone, read-only)   ← butuh RJ-BIL-BE-001  ✅ siap
   ├── RJ-BIL-FE-002 (batas klinis vs finansial)        ← butuh BE-002/003 ✅ siap; BE-004 ⛔ menyusul
   │      ├── RJ-BIL-FE-004 (financial action + approval)  ← butuh BE-006  ✅ siap
   │      ├── RJ-BIL-FE-005 (reconciliation + outage)      ← butuh BE-007  ✅ siap
   │      └── RJ-BIL-FE-006 (klaim/settlement manual)      ← butuh BE-008  ⛔
   └── RJ-BIL-FE-003 (allocation + patient responsibility) ← butuh BE-005  ⛔

RJ-BIL-FE-007 (coverage gap + regression UI) — paling akhir, menunggu FE-001 s.d. FE-006
```

**Yang boleh paralel.** Setelah `RJ-BIL-FE-001` selesai, `FE-002` dan `FE-003` tidak saling
bergantung. Setelah `FE-002` selesai, `FE-004`, `FE-005`, dan `FE-006` juga tidak saling
bergantung. Yang mengikat pada tiap task adalah baris **Dependency**; diagram di atas adalah
ringkasannya.

---

## 4. Task

### ✅ `RJ-BIL-FE-001` — Petugas dapat membaca tagihan satu kunjungan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI `2026-08-28`.** Layar dibuat di bawah `RJ-BIL-DEC-013`; `next build` lulus (`exit 0`) dan route `[encounterId]` terbukti dihasilkan; 29 test baru lulus. `FRONTEND_AUTHORITY` visual dan `SECURITY_PRIVACY_SIGNOFF` tetap `OPEN` |
| **Outcome** | Menyediakan consumer read-only Folio dan milestone status, sehingga petugas dapat melihat isi tagihan satu kunjungan tanpa satu pun angka dapat diubah dari layar |
| **Trace** | `RJ-BIL-GATE-DEC-001`, `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-020` berstatus `Missing` |
| **Kontrak** | API/Validation `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` |
| **Reuse** | Konvensi Axios, Redux, dan penanganan loading/error yang sudah ada |
| **Scope** | Query folio berdasarkan encounter atau id; tampilan charge line dan component; processing outcome; refresh; penjaga stale response |
| **Dependency** | `RJ-BIL-BE-001`; **`RJ-BIL-BE-007`** — baris ini semula hanya menyebut `BE-001`, padahal `processing outcome` pada Scope hanya tersedia lewat `GET /reconciliation/processing-status` milik `BE-007`. Dicatat sebagai delta, **bukan** disunting sepihak sebagai kontrak; frontend API authority |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. `OutcomeUnknown` **bukan** failed dan **bukan** success. 2. `404` ditampilkan sebagai tidak ditemukan, **bukan** kosong. 3. `409` menampilkan konflik beserta reload terkontrol. 4. UUID **bukan** satu-satunya label di layar |
| **Verifikasi** | ✅ **Keempat kriteria terbukti test.** 29 test unit lulus; seluruh suite `67 lulus, 0 gagal`; lint `0 masalah`; build `exit 0`. **Component render test belum mungkin** — harness project memakai `node --test` tanpa `@testing-library`; batas ini menjadi cakupan `RJ-BIL-FE-007` |
| **Risiko/pemilik** | Frontend authority |
| **DoD** | ✅ Consumer kontrak teruji; **tidak ada satu pun mutasi finansial dari frontend** — service layer sengaja hanya memuat operasi baca |
| **Bukti** | [fe-rj-bil-001-baca-tagihan-satu-kunjungan.md](../task/report/frontend/fe-rj-bil-001-baca-tagihan-satu-kunjungan.md) |

---

### ✅ `RJ-BIL-FE-002` — Pesanan klinis tidak pernah tampil sebagai lunas

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI `2026-08-28` untuk bagian Resep, Tindakan, dan Laboratorium.** `next build` lulus (`exit 0`), route `clinical-boundary/[encounterId]` terbukti dihasilkan, 21 test baru lulus. **Bagian Radiologi tetap ⛔** dan sengaja tidak dikerjakan — Radiologi belum terdaftar pada `BillingSourceContract` backend dan `RJ-BIL-BE-004` masih terblokir. Ketiadaannya **diumumkan di layar**, bukan didiamkan |
| **Outcome** | Menampilkan clinical milestone dan financial boundary, sehingga petugas tidak pernah salah membaca pesanan klinis sebagai tagihan yang sudah lunas |
| **Trace** | `RJ-BIL-GATE-DEC-001`, `003`, `004`, `007`; `RJ-BIL-CAP-019` berstatus `Missing` |
| **Kontrak** | State `RJ-BIL-STATE-001@1.0.0` |
| **Reuse** | Tab resep dan tindakan pada antrean dokter yang sudah ada — `RJ-BIL-CAP-018`. **Tidak boleh** dijadikan financial source of truth |
| **Scope** | Membedakan order, fulfillment, milestone, charge, dan projection. Status Lab dan Radiologi ditampilkan **hanya bila** endpoint-nya benar-benar tersedia |
| **Dependency** | `RJ-BIL-FE-001`; `RJ-BIL-BE-002`, `BE-003`, `BE-004` |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; perbedaan kelima jenis status **tidak** `DEV_DISCRETION` |
| **Acceptance criteria** | 1. UI **tidak** menampilkan order sebagai `Paid`. 2. Sumber dan versi status terlihat. 3. Stale response ditolak, tidak menimpa state yang lebih baru |
| **Verifikasi** | ✅ **Ketiga kriteria terbukti test.** 21 test unit lulus; seluruh suite `88 lulus, 0 gagal`; lint `0 masalah`; build `exit 0`. **Component render test belum mungkin** — harness `node --test` tanpa `@testing-library`; cakupan `RJ-BIL-FE-007` |
| **Risiko/pemilik** | Clinical, Pharmacy, dan Frontend |
| **DoD** | 🟡 Batas klinis–finansial **belum ditinjau domain owner**. Implementasinya selesai dan teruji, tetapi tinjauan domain owner adalah syarat yang hanya dapat dipenuhi manusia |
| **Bukti** | [fe-rj-bil-002-batas-klinis-dan-finansial.md](../task/report/frontend/fe-rj-bil-002-batas-klinis-dan-finansial.md) |
| **Temuan** | `PrescriptionResponse.paymentStatus` **masih mengirim nilai `Lunas`** walau `RJ-BIL-BE-002` sudah mencabut kewenangan finansial modul klinis — endpoint yang **menulis** dihapus, yang **membaca** belum. Layar menjinakkannya; keputusan menarik kolom itu dari payload adalah wewenang pemilik |

---

### ⛔ `RJ-BIL-FE-003` — Pembagian penanggung terlihat beserta sisanya

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-005` terblokir menunggu keputusan pemilik atas `RJ-BIL-CONFLICT-001`. Selama bentuk allocation belum diputuskan, layar ini **tidak dapat dirancang** |
| **Outcome** | Menampilkan allocation dan patient responsibility, sehingga pasien dan kasir dapat melihat siapa menanggung berapa, dan berapa sisanya |
| **Trace** | `RJ-BIL-GATE-DEC-002`; `RJ-BIL-CAP-020` berstatus `Missing`; `RJ-BIL-CONFLICT-001` |
| **Kontrak** | API/Validation `RJ-BIL-API-001@1.0.0` |
| **Reuse** | Referensi penanggung dan encounter yang sudah ada |
| **Scope** | Tampilan read-only versi allocation, nominal per penanggung, sisa, alasan, dan histori. **Tanpa** kemampuan menimpa |
| **Dependency** | `RJ-BIL-FE-001`; `RJ-BIL-BE-005` |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Total allocation + patient responsibility = net eligible. 2. Versi yang sudah digantikan tetap dapat dilihat |
| **Verifikasi** | Component/property/API test |
| **Risiko/pemilik** | Billing, Payer, dan Frontend |
| **DoD** | Tampilan multi-payer ditinjau; **tidak ada keputusan penanggung baru yang lahir dari UI** |

---

### `RJ-BIL-FE-004` — Tindakan finansial diajukan, bukan langsung dijalankan

| Field | Isi |
| --- | --- |
| **Status** | **Belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-006` sudah ✅ selesai `2026-08-27`, dan wewenang tulisnya sudah diberikan `RJ-BIL-DEC-013`. **Siap dikerjakan**. **Matriks nominal approval `RJ-BIL-OQ-004` belum ditetapkan**, sehingga tindakan yang bergantung ambang akan berbalas `BlockedByPolicyConfiguration` — layar wajib menampilkannya sebagai keadaan yang sah, bukan sebagai galat sistem |
| **Outcome** | Menyediakan form financial action dan status approval, sehingga pembatalan, koreksi, dan pengembalian uang selalu terlihat sebagai **permintaan** — bukan sebagai perubahan yang sudah terjadi |
| **Trace** | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `RJ-BIL-CAP-015` |
| **Kontrak** | Permission/State `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` |
| **Reuse** | Pola permission dan aksi yang sudah ada |
| **Scope** | Pengiriman alasan dan nominal; status menunggu persetujuan; keputusan checker; galat self-approval; rujukan audit |
| **Dependency** | `RJ-BIL-FE-002`; `RJ-BIL-BE-006`. Owner Workflow tidak lagi termasuk sejak `RJ-BIL-DEC-011`; Security owner tetap untuk sign-off production |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; aturan peran pada arsitektur bagian 4 **tidak** `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Permintaan yang menunggu persetujuan **tidak** mengubah canonical charge. 2. Self-approval tampil sebagai galat. 3. Kirim ulang tidak menghasilkan permintaan kembar |
| **Verifikasi** | Form/API/accessibility/security test |
| **Risiko/pemilik** | Finance, Security, dan Frontend |
| **DoD** | Permission matrix dikonsumsi persis; **tidak ada satu pun jalan pintas tersembunyi** |

---

### `RJ-BIL-FE-005` — Gangguan pemrosesan terlihat dan tidak memancing kirim ulang

| Field | Isi |
| --- | --- |
| **Status** | **Belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-007` sudah ✅ selesai beserta delapan endpoint rekonsiliasinya, dan wewenang tulisnya sudah diberikan `RJ-BIL-DEC-013`. **Siap dikerjakan** |
| **Outcome** | Menampilkan reconciliation dan outage state, sehingga petugas tahu apa yang sedang terjadi dan apa tindakan berikutnya — bukan menekan tombol kirim berulang kali |
| **Trace** | `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-017`, `RJ-BIL-CAP-021` |
| **Kontrak** | Integration `RJ-BIL-INT-001@1.0.0` |
| **Reuse** | Pola loading, error, dan retry yang sudah ada |
| **Scope** | `OutcomeUnknown`, rekonsiliasi tertunda, pemilik case dan tindakan berikutnya, refresh pemulihan. **Tanpa** retry membabi buta |
| **Dependency** | `RJ-BIL-FE-002`; `RJ-BIL-BE-007` |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; penanganan timeout dan stale response pada arsitektur bagian 3 **tidak** `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Timeout **mempertahankan** request identity; idempotency key tidak diganti sebelum status query selesai. 2. Komponen yang gagal **tidak** ditampilkan sebagai nol. 3. Penutupan folio yang terblokir terlihat alasannya |
| **Verifikasi** | Failure/mock/recovery/accessibility test |
| **Risiko/pemilik** | Billing, Integration, dan Frontend |
| **DoD** | UX pemulihan disetujui; **tidak ada penyelesaian otomatis tanpa manusia** |

---

### ⛔ `RJ-BIL-FE-006` — Klaim manual terlihat apa adanya, tanpa mengaku sukses eksternal

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-008` terblokir menunggu `RJ-BIL-BE-005` |
| **Outcome** | Menampilkan status manual payer, klaim, dan settlement, sehingga tidak ada satu pun layar yang menyiratkan sistem penanggung eksternal sudah menjawab |
| **Trace** | `RJ-BIL-GATE-DEC-009`; `RJ-BIL-CAP-022`; `RJ-BIL-DEP-009` berstatus `INACTIVE` |
| **Kontrak** | Integration `RJ-BIL-INT-001@1.0.0` |
| **Reuse** | Komponen status alur manual yang sudah ada |
| **Scope** | Label `ManualOperator`; pemisahan klaim dan pembayaran; penanda adapter tidak aktif |
| **Dependency** | `RJ-BIL-FE-002`; `RJ-BIL-BE-008` |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; ketiadaan tombol aktivasi adapter **tidak** `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Klaim yang disetujui tetap tampil `PaymentPending`. 2. Adapter eksternal **tidak memiliki** aksi aktivasi di layar mana pun |
| **Verifikasi** | Component/API/security test |
| **Risiko/pemilik** | Payer, Finance, dan Frontend |
| **DoD** | Consumer alur manual ditinjau |

---

### ⛔ `RJ-BIL-FE-007` — Setiap acceptance criteria UI kritis punya bukti test

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Menunggu `RJ-BIL-FE-001` s.d. `RJ-BIL-FE-006` selesai lebih dulu |
| **Outcome** | Menutup coverage gap dan regression UI, sehingga tidak ada satu pun layar yang berstatus selesai tanpa bukti |
| **Trace** | `RJ-BIL-CAP-021` berstatus `Missing` pada snapshot audit |
| **Kontrak** | Acceptance `testing/acceptance-test-matrix.md` |
| **Reuse** | Konvensi test frontend yang sudah ada |
| **Scope** | Test untuk kirim ganda, stale response, permission, privasi, serta perilaku responsif dan accessibility |
| **Dependency** | `RJ-BIL-FE-001` s.d. `RJ-BIL-FE-006` |
| **Wewenang UI** | Tidak ada |
| **Acceptance criteria** | 1. Setiap acceptance criteria UI kritis punya bukti test **atau** pemilik gap-nya bernama |
| **Verifikasi** | Laporan test dan tinjauan traceability |
| **Risiko/pemilik** | QA dan frontend authority |
| **DoD** | Laporan cakupan lengkap; gap yang diketahui sudah ada pemiliknya |

> **Task ini sering diperlakukan sebagai formalitas penutup.** Pada snapshot audit tidak ditemukan
> satu pun test frontend yang relevan — `RJ-BIL-CAP-021` berstatus `Missing`. Ini justru satu-satunya
> tempat lubang cakupan UI ketahuan sebelum layar dipakai pasien sungguhan.

---

## 5. Gerbang yang masih terbuka

| Gerbang | Keadaannya | Menahan |
| --- | --- | --- |
| ~~**`IMPLEMENTATION_AUTHORITY` frontend**~~ | **DITUTUP `2026-08-28` oleh `RJ-BIL-DEC-013`.** `GRANTED`; `BUILDER_EXECUTION` `AUTHORIZED` untuk `FE-001`, `FE-002` bagian Lab, `FE-004`, dan `FE-005` | `FE-003`, `FE-006`, `FE-007` tetap `NOT_AUTHORIZED` karena endpoint pasangannya belum ada |
| **Frontend authority / UI visual authority** | Belum ditunjuk | Route final, menu, sidebar, modal/drawer, warna status, dan pustaka komponen tetap `DEV_DISCRETION` |
| Endpoint `RJ-BIL-BE-005` tersedia | ⛔ Terblokir `RJ-BIL-CONFLICT-001` | `RJ-BIL-FE-003` |
| ~~Endpoint `RJ-BIL-BE-006` tersedia~~ | **DITUTUP `2026-08-27`.** `RJ-BIL-BE-006` selesai; `46` test lulus dan migration-nya diterapkan | — |
| Endpoint `RJ-BIL-BE-008` tersedia | ⛔ Terblokir menunggu `RJ-BIL-BE-005` | `RJ-BIL-FE-006` |
| Endpoint Radiologi `RJ-BIL-BE-004` tersedia | ⛔ Terblokir; owner `RadiologyManagement` belum ditunjuk | Bagian Radiologi pada `RJ-BIL-FE-002` — bagian Lab tidak tertahan |
| Security/Privacy owner | `OPEN` | **Tidak menahan.** Aturan privasi yang sudah tertulis tetap berlaku dan tetap diuji |
| `RJ-BIL-DEP-009` adapter payer eksternal | Kontrak, kredensial, sandbox/UAT, dan bukti rekonsiliasi belum ada | Tombol aktivasi produksi **dilarang ada** di layar mana pun |

---

## 6. Yang sengaja tidak ada di roadmap ini

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Tombol aktivasi adapter payer eksternal | Dilarang sampai `RJ-BIL-DEP-009` selesai — arsitektur frontend bagian 7 |
| Frontend sebagai sumber kebenaran finansial | Angka kanonik hanya lahir dari Billing. Layar membaca, tidak menetapkan |
| Pembuatan folio dari frontend | Keadaan kosong menjelaskan folio belum terbentuk; ia **tidak** membuatkannya |
| Penyelesaian rekonsiliasi otomatis | `RJ-BIL-FE-005` menampilkan dan menugaskan; manusia yang memutuskan |
| Keputusan penanggung baru dari layar | `RJ-BIL-FE-003` read-only terhadap allocation |
| Route final, menu, sidebar, warna, dan pustaka komponen | Belum ada UI authority yang sah. Implementer mengusulkan opsi, lalu menunggu |

Keenam butir itu adalah **keadaan yang disengaja**, bukan cakupan yang terlupa.

---

## 7. Aturan eksekusi dan handoff builder

Roadmap ini sudah disetujui untuk eksekusi task, dan wewenang tulisnya **sudah diberikan sebagian**
oleh `RJ-BIL-DEC-013` pada `2026-08-28` — terbatas pada `RJ-BIL-FE-001`, `RJ-BIL-FE-002` bagian Lab,
`RJ-BIL-FE-004`, dan `RJ-BIL-FE-005`. Setiap handoff ke `build-module-frontend` wajib menyertakan:

| Yang wajib disertakan | Contoh |
| --- | --- |
| Task ID | `RJ-BIL-FE-001` |
| Approval task | `APPROVED_FOR_EXECUTION` pada `2026-08-21` |
| Kontrak terkunci | `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` |
| Bukti endpoint benar-benar dapat dipanggil | Bukan sekadar tercantum pada dokumen kontrak |
| Wewenang UI | Apa yang sudah disetujui, dan apa yang tetap `DEV_DISCRETION` |
| Wewenang tulis frontend | Eksplisit — `GRANTED` hanya untuk keempat task yang disebut `RJ-BIL-DEC-013`; sisanya `NOT_AUTHORIZED` |
| Bukti acceptance yang diminta | Sesuai baris **Verifikasi** dan **DoD** task tersebut |

> **Tidak satu pun task di dokumen ini memberi izin** mengubah backend, mengaktifkan adapter
> eksternal, atau mengambil keputusan produk yang belum disetujui.
