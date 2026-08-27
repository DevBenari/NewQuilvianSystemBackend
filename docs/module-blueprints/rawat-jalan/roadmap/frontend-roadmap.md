# Roadmap Delivery Frontend — Modul Rawat Jalan Billing

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
module_name: Dokter / Rawat Jalan Billing
module_slug: rawat-jalan
module_prefix: RJ-BIL
repository: V2QuilvianSystemFrontendDev
blueprint_revision: 11
roadmap_revision: 1
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
implementation_authority: NOT_GRANTED
builder_execution: NOT_AUTHORIZED
external_adapter: "RJ-BIL-DEP-009 = INACTIVE / OUT OF CURRENT DELIVERY SCOPE"
task_count: 7
progress: "0 dari 7 task frontend dimulai per 2026-08-27"
```

---

## 0. Peringatan yang tidak boleh dilewati

> **Roadmap ini berstatus `APPROVED_FOR_EXECUTION` sejak 2026-08-21.** Ketujuh task
> `RJ-BIL-FE-001` s.d. `RJ-BIL-FE-007` sudah disetujui pemilik pekerjaan.
>
> Approval itu **bukan** izin menulis `.jsx`. `IMPLEMENTATION_AUTHORITY` masih `NOT_GRANTED` dan
> `BUILDER_EXECUTION` masih `NOT_AUTHORIZED` untuk **seluruh** task frontend. Setiap task tetap
> memerlukan handoff task dan wewenang tulis frontend pada waktu eksekusi.
>
> **Tidak satu pun task di bawah boleh mengaktifkan `RJ-BIL-DEP-009`.** Adapter payer eksternal
> tidak boleh punya tombol aktivasi produksi di layar mana pun.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya | Syarat |
| :---: | --- | --- |
| ✅ | **SELESAI** | Layar sudah dibuat, build lulus, dan test acceptance-nya lulus dengan bukti tercatat |
| 🟡 | **KODE SIAP, BELUM DI-BUILD** | `.jsx` sudah ditulis, tetapi build dan test belum dijalankan — sehingga belum ada bukti apa pun bahwa layar itu berjalan |
| ⛔ | **TERBLOKIR** | Endpoint backend pasangannya belum ada, atau task frontend pendahulunya belum selesai |
| tanpa tanda | **Belum dikerjakan** | Endpoint backend pasangannya sudah ada; yang belum ada adalah wewenang tulis frontend |

**Keadaan hari ini, 27 Agustus 2026.**

| Hal | Keadaannya |
| --- | --- |
| Task frontend selesai | **0 dari 7.** Belum satu pun dimulai |
| Task yang backend-nya sudah siap | **3** — `RJ-BIL-FE-001`, `RJ-BIL-FE-002` (bagian Lab), `RJ-BIL-FE-005` |
| Task yang backend-nya masih terblokir | **4** — `RJ-BIL-FE-003`, `RJ-BIL-FE-004`, `RJ-BIL-FE-006`, `RJ-BIL-FE-007` |
| Wewenang tulis frontend | `NOT_GRANTED` untuk seluruh task |

Keadaan frontend hari ini, dari capability map pada commit `ab4bd83`:

| Hal | Keadaannya | Bukti |
| --- | --- | --- |
| Workspace Dokter/Rawat Jalan | **Ada dan dapat dijangkau.** Route, antrean, SOAP, CPPT, resep, tindakan, surat keterangan, serta penanganan loading dan error sudah tersedia | `RJ-BIL-CAP-002` — *Ready to reuse* |
| Tab resep dan tindakan | **Ada.** Draft, autosave, dan finalize sudah terintegrasi | `RJ-BIL-CAP-018` — *Ready to reuse*. **Tidak boleh** dijadikan financial source of truth |
| Tab order Lab dan Radiologi | **Tidak ada.** Yang muncul hanya label CPPT dan metadata master, bukan journey order operasional | `RJ-BIL-CAP-019` — *Missing*; `RJ-BIL-CONFLICT-003` |
| Layar billing, folio, payer split, correction status | **Tidak ada satu pun consumer** | `RJ-BIL-CAP-020` — *Missing* |
| Test frontend yang relevan | **Tidak ditemukan** pada snapshot audit | `RJ-BIL-CAP-021` — *Missing* |

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
| `RJ-BIL-FE-001` | `RJ-BIL-BE-001` | ✅ **Selesai** — boleh dikerjakan begitu wewenang tulis diberikan |
| `RJ-BIL-FE-002` | `RJ-BIL-BE-002`, `BE-003`, `BE-004` | ✅ Resep, tindakan, dan Lab selesai; ⛔ **Radiologi terblokir**. Bagian Radiologi menyusul |
| `RJ-BIL-FE-003` | `RJ-BIL-BE-005` | ⛔ **Terblokir** — menunggu keputusan `RJ-BIL-CONFLICT-001` |
| `RJ-BIL-FE-004` | `RJ-BIL-BE-006` | ⛔ **Terblokir** — `BUILDER_EXECUTION` masih `NOT_AUTHORIZED` |
| `RJ-BIL-FE-005` | `RJ-BIL-BE-007` | ✅ **Selesai** — boleh dikerjakan begitu wewenang tulis diberikan |
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
| **F4 — Tindakan finansial dan persetujuannya** | Permintaan yang menunggu persetujuan tidak mengubah angka; self-approval tampil sebagai galat | ⛔ `RJ-BIL-FE-004` |
| **F5 — Klaim manual per penanggung** | Klaim disetujui tetap `PaymentPending`; adapter eksternal tidak punya tombol aktivasi | ⛔ `RJ-BIL-FE-006` |
| **F6 — Kesiapan sebelum sign-off** | Setiap acceptance criteria UI kritis punya bukti test atau pemilik gap-nya | ⛔ `RJ-BIL-FE-007` |

### Urutan dependency

```text
RJ-BIL-FE-001 (consumer folio + milestone, read-only)   ← butuh RJ-BIL-BE-001  ✅ siap
   ├── RJ-BIL-FE-002 (batas klinis vs finansial)        ← butuh BE-002/003 ✅ siap; BE-004 ⛔ menyusul
   │      ├── RJ-BIL-FE-004 (financial action + approval)  ← butuh BE-006  ⛔
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

### `RJ-BIL-FE-001` — Petugas dapat membaca tagihan satu kunjungan

| Field | Isi |
| --- | --- |
| **Status** | **Belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-001` sudah ✅ selesai; yang belum ada adalah wewenang tulis frontend |
| **Outcome** | Menyediakan consumer read-only Folio dan milestone status, sehingga petugas dapat melihat isi tagihan satu kunjungan tanpa satu pun angka dapat diubah dari layar |
| **Trace** | `RJ-BIL-GATE-DEC-001`, `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-020` berstatus `Missing` |
| **Kontrak** | API/Validation `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` |
| **Reuse** | Konvensi Axios, Redux, dan penanganan loading/error yang sudah ada |
| **Scope** | Query folio berdasarkan encounter atau id; tampilan charge line dan component; processing outcome; refresh; penjaga stale response |
| **Dependency** | `RJ-BIL-BE-001`; frontend API authority |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. `OutcomeUnknown` **bukan** failed dan **bukan** success. 2. `404` ditampilkan sebagai tidak ditemukan, **bukan** kosong. 3. `409` menampilkan konflik beserta reload terkontrol. 4. UUID **bukan** satu-satunya label di layar |
| **Verifikasi** | Component/API/mock test; pemeriksaan accessibility |
| **Risiko/pemilik** | Frontend authority |
| **DoD** | Consumer kontrak teruji; **tidak ada satu pun mutasi finansial dari frontend** |

---

### `RJ-BIL-FE-002` — Pesanan klinis tidak pernah tampil sebagai lunas

| Field | Isi |
| --- | --- |
| **Status** | **Belum dikerjakan.** Bagian resep, tindakan, dan Lab sudah dapat dikerjakan — `RJ-BIL-BE-002` dan `BE-003` ✅ selesai. **Bagian Radiologi menyusul**, karena `RJ-BIL-BE-004` masih ⛔ terblokir |
| **Outcome** | Menampilkan clinical milestone dan financial boundary, sehingga petugas tidak pernah salah membaca pesanan klinis sebagai tagihan yang sudah lunas |
| **Trace** | `RJ-BIL-GATE-DEC-001`, `003`, `004`, `007`; `RJ-BIL-CAP-019` berstatus `Missing` |
| **Kontrak** | State `RJ-BIL-STATE-001@1.0.0` |
| **Reuse** | Tab resep dan tindakan pada antrean dokter yang sudah ada — `RJ-BIL-CAP-018`. **Tidak boleh** dijadikan financial source of truth |
| **Scope** | Membedakan order, fulfillment, milestone, charge, dan projection. Status Lab dan Radiologi ditampilkan **hanya bila** endpoint-nya benar-benar tersedia |
| **Dependency** | `RJ-BIL-FE-001`; `RJ-BIL-BE-002`, `BE-003`, `BE-004` |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; perbedaan kelima jenis status **tidak** `DEV_DISCRETION` |
| **Acceptance criteria** | 1. UI **tidak** menampilkan order sebagai `Paid`. 2. Sumber dan versi status terlihat. 3. Stale response ditolak, tidak menimpa state yang lebih baru |
| **Verifikasi** | UI state test; error test; accessibility test |
| **Risiko/pemilik** | Clinical, Pharmacy, dan Frontend |
| **DoD** | Batas klinis–finansial ditinjau domain owner |

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

### ⛔ `RJ-BIL-FE-004` — Tindakan finansial diajukan, bukan langsung dijalankan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR — belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-006` terblokir; `BUILDER_EXECUTION` masih `NOT_AUTHORIZED` dan matriks nominal approval `RJ-BIL-OQ-004` belum ditetapkan |
| **Outcome** | Menyediakan form financial action dan status approval, sehingga pembatalan, koreksi, dan pengembalian uang selalu terlihat sebagai **permintaan** — bukan sebagai perubahan yang sudah terjadi |
| **Trace** | `RJ-BIL-GATE-DEC-006`; `RJ-BIL-CAP-014`, `RJ-BIL-CAP-015` |
| **Kontrak** | Permission/State `RJ-BIL-PERM-001@1.0.0`, `RJ-BIL-STATE-001@1.0.0` |
| **Reuse** | Pola permission dan aksi yang sudah ada |
| **Scope** | Pengiriman alasan dan nominal; status menunggu persetujuan; keputusan checker; galat self-approval; rujukan audit |
| **Dependency** | `RJ-BIL-FE-002`; `RJ-BIL-BE-006`; Workflow dan Security owner |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; aturan peran pada arsitektur bagian 4 **tidak** `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Permintaan yang menunggu persetujuan **tidak** mengubah canonical charge. 2. Self-approval tampil sebagai galat. 3. Kirim ulang tidak menghasilkan permintaan kembar |
| **Verifikasi** | Form/API/accessibility/security test |
| **Risiko/pemilik** | Finance, Security, dan Frontend |
| **DoD** | Permission matrix dikonsumsi persis; **tidak ada satu pun jalan pintas tersembunyi** |

---

### `RJ-BIL-FE-005` — Gangguan pemrosesan terlihat dan tidak memancing kirim ulang

| Field | Isi |
| --- | --- |
| **Status** | **Belum dikerjakan.** Backend pasangannya `RJ-BIL-BE-007` sudah ✅ selesai beserta delapan endpoint rekonsiliasinya; yang belum ada adalah wewenang tulis frontend |
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
| **`IMPLEMENTATION_AUTHORITY` frontend** | `NOT_GRANTED`; `BUILDER_EXECUTION` `NOT_AUTHORIZED` | **Seluruh task**, termasuk ketiga task yang backend-nya sudah siap |
| **Frontend authority / UI visual authority** | Belum ditunjuk | Route final, menu, sidebar, modal/drawer, warna status, dan pustaka komponen tetap `DEV_DISCRETION` |
| Endpoint `RJ-BIL-BE-005` tersedia | ⛔ Terblokir `RJ-BIL-CONFLICT-001` | `RJ-BIL-FE-003` |
| Endpoint `RJ-BIL-BE-006` tersedia | ⛔ Terblokir; `BUILDER_EXECUTION` `NOT_AUTHORIZED` | `RJ-BIL-FE-004` |
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

Roadmap ini sudah disetujui untuk eksekusi task, tetapi wewenang tulisnya **belum diberikan**.
Setiap handoff ke `build-module-frontend` wajib menyertakan:

| Yang wajib disertakan | Contoh |
| --- | --- |
| Task ID | `RJ-BIL-FE-001` |
| Approval task | `APPROVED_FOR_EXECUTION` pada `2026-08-21` |
| Kontrak terkunci | `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` |
| Bukti endpoint benar-benar dapat dipanggil | Bukan sekadar tercantum pada dokumen kontrak |
| Wewenang UI | Apa yang sudah disetujui, dan apa yang tetap `DEV_DISCRETION` |
| Wewenang tulis frontend | Eksplisit — hari ini masih `NOT_GRANTED` |
| Bukti acceptance yang diminta | Sesuai baris **Verifikasi** dan **DoD** task tersebut |

> **Tidak satu pun task di dokumen ini memberi izin** mengubah backend, mengaktifkan adapter
> eksternal, atau mengambil keputusan produk yang belum disetujui.
