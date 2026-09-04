# Laporan Perubahan Frontend — `RJ-BIL-FE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| TASK ID | `RJ-BIL-FE-001` — Petugas dapat membaca tagihan satu kunjungan |
| TASK TYPE | Implementasi task frontend approved dari roadmap canonical |
| COMPLEXITY | `MEDIUM` |
| MODEL | `claude-opus-5` |
| TASK MODE | `MODULE BLUEPRINT` |
| WRITE TARGET | `V2QuilvianSystemFrontendDev/src/` dan `tests/unit/`; laporan ini di `NewQuilvianSystemBackend/docs/` |
| Wewenang tulis | `RJ-BIL-DEC-013`, 28 Agustus 2026 — `IMPLEMENTATION_AUTHORITY: GRANTED`, `BUILDER_EXECUTION: AUTHORIZED` untuk `FE-001` |
| Trace | `RJ-BIL-GATE-DEC-001`, `RJ-BIL-GATE-DEC-008`; `RJ-BIL-CAP-020` berstatus `Missing` |
| Contract version | `RJ-BIL-API-001@1.0.0`, `RJ-BIL-VAL-001@1.0.0` — **tidak berubah**. Layar ini hanya mengonsumsi |
| Backend pasangan | `RJ-BIL-BE-001` (folio) dan `RJ-BIL-BE-007` (status pemrosesan) |
| Branch / HEAD frontend | `QuilvianDevV2` / `bd31dc9` |
| Tanggal | 28 Agustus 2026 |
| Status | **SELESAI** — keempat acceptance criteria terbukti; `FRONTEND_AUTHORITY` visual dan `SECURITY_PRIVACY_SIGNOFF` tetap `OPEN` |

> **Arti ✅ pada task ini.** Layar sudah dibuat, `next build` lulus, dan test acceptance-nya lulus
> dengan bukti tercatat. Itu **bukan** pernyataan bahwa layar ini boleh dipakai melayani pasien.
> Wewenang visual atas route, menu, dan bentuk komponen masih `OPEN`, dan layar ini belum masuk
> navigasi mana pun.

---

## 1. Temuan preflight yang harus dibaca lebih dulu

**Kedua SHA frontend yang tercatat pada governance sudah basi.**

| Sumber | SHA tercatat | Jarak ke `HEAD` |
| --- | --- | --- |
| `blueprint-manifest.md` | `ab4bd836` | **11 commit di belakang** |
| `MODULE-STATUS.md` | `32db4acb` | 1 commit di belakang |
| Pohon sebenarnya | `bd31dc99` | — (di-commit 28 Agustus 2026) |

Artinya capability map yang menjadi dasar roadmap frontend disusun terhadap pohon yang berbeda.
Klaim terpentingnya karena itu diverifikasi ulang langsung ke pohon saat ini, bukan dipercaya dari
snapshot:

```
grep -ril "folio" src   ->  nol hasil
```

`RJ-BIL-CAP-020` *(layar billing, folio, payer split, correction status — tidak ada satu pun
consumer)* **masih benar**. `FE-001` memang greenfield, dan dasar kerjanya sah.

Selisih SHA itu **sengaja tidak disamakan** dari task ini. Menebak snapshot mana yang benar adalah
wewenang pemilik frontend, dan menyamakannya sepihak akan membuat pohon yang tidak pernah diaudit
tampak seolah sudah diverifikasi.

---

## 2. Delta kontrak yang ditemukan

Baris **Dependency** `RJ-BIL-FE-001` pada roadmap berbunyi `RJ-BIL-BE-001`. Namun baris **Scope**
task yang sama menuntut `processing outcome` ikut ditampilkan.

Data itu **tidak ada** pada `BillingFolioDetailResponse`. Ia hanya tersedia lewat
`GET /reconciliation/processing-status`, milik `RJ-BIL-BE-007`.

| Yang dituntut Scope | Tersedia di | Milik task |
| --- | --- | --- |
| Folio, charge line, component | `GET /folios/by-encounter/{id}` dan `GET /folios/{id}` | `RJ-BIL-BE-001` |
| **Processing outcome** | `GET /reconciliation/processing-status` | **`RJ-BIL-BE-007`** |

Keempat parameter yang dituntut endpoint itu — `sourceContext`, `milestoneFactId`,
`milestoneFactVersion`, `effectType` — seluruhnya **sudah dikirim** pada setiap baris tagihan oleh
`BE-001`, sehingga layar tidak perlu menebak apa pun. Dependency-nya dapat dipenuhi tanpa
menyentuh backend.

**Ini dicatat sebagai delta, bukan diperbaiki diam-diam.** Baris Dependency roadmap kurang
menyebut `RJ-BIL-BE-007`. Pemilik roadmap yang berwenang melengkapinya.

---

## 3. Yang dibangun

Sembilan berkas baru, satu berkas disunting.

| Lapisan | Berkas | Isi |
| --- | --- | --- |
| Constants | `lib/constants/health-services/billing-management/billing-folio-constant.jsx` | Endpoint, peta enum, label Indonesia, nada status |
| Utils | `utils/health-services/billing-management/billing-folio-utils.jsx` | Normalisasi, klasifikasi kegagalan HTTP, dua penjaga respons usang, format |
| Service | `lib/services/health-services/billing-management/billing-folio.service.js` | Tiga operasi **baca**, tanpa satu pun operasi tulis |
| Slice | `lib/state/slice/health-services/billing-management/billing-folio-slice.jsx` | Thunk, penjaga usang, kegagalan berjenis |
| Store | `lib/state/store.jsx` — **disunting** | Registrasi `billingFolio` |
| Hook | `lib/hooks/health-services/billing-management/use-billing-folio-detail.jsx` | Controller baca, pembatalan saat unmount |
| View | `components/view/.../billing-folio-detail-view.jsx` | Komposisi layar |
| View | `components/view/.../components/billing-folio-summary.jsx` | Ringkasan folio |
| View | `components/view/.../components/billing-charge-line-detail.jsx` | Komponen perhitungan dan hasil pemrosesan |
| View | `components/view/.../components/billing-folio-failure-notice.jsx` | Keadaan gagal menurut jenisnya |
| Style | `style/health-services/billing-management/billing-folio.module.css` | CSS Module, token mengikuti Emergency Triage |
| Route | `app/health-services/billing-management/billing-folio/[encounterId]/page.jsx` | Entry point tipis |
| Test | `tests/unit/billing-folio-utils.test.mjs` | 29 test |

Tidak ada arsitektur state, HTTP client, atau abstraksi paralel yang ditambahkan. Seluruh request
lewat `InstanceAxios`, seluruh state lewat Redux Toolkit, seluruh komponen dasar diambil dari
`components/features/base-features/`.

---

## 4. Empat keputusan yang perlu dipertahankan

### 4.1 Enum tiba sebagai angka, bukan string

`Program.cs` **tidak** memasang `JsonStringEnumConverter`. Seluruh enum backend karena itu tiba
sebagai angka. Membandingkan `status === "Open"` akan selalu gagal **diam-diam** — tanpa galat,
tanpa peringatan, hanya label yang salah. Peta enum di constants menyimpan angkanya, dan ada test
yang mengunci perilaku itu.

### 4.2 Keadaan belum-pasti punya nada sendiri

Kriteria penerimaan 1 menuntut `OutcomeUnknown` **bukan** gagal dan **bukan** berhasil. Karena itu
nada tampilan ada **empat**, bukan dua:

| Nada | Anggota |
| --- | --- |
| `netral` | `Received`, `InProgress` |
| `berhasil` | `Succeeded` |
| **`belum-pasti`** | `PartialOutcome`, **`OutcomeUnknown`** |
| `gagal` | `FailedBeforeEffect`, `RejectedValidation`, `TransientFailure`, `PermanentFailure` |

Nilai enum yang **tidak dikenal** sengaja jatuh ke `belum-pasti`, bukan ke `gagal`. Backend dapat
menambah anggota enum lebih dulu daripada frontend, dan menebak "gagal" atas sesuatu yang belum
dipahami adalah tebakan yang memancing kirim ulang — persis yang dilarang `GATE-DEC-008`.

Keterangan yang menemani nada `belum-pasti` menyatakan larangannya secara eksplisit: *"Jangan kirim
ulang; tunggu rekonsiliasi."*

### 4.3 Dua lapis penjaga respons usang, bukan satu

| Lapis | Menjaga dari | Cara |
| --- | --- | --- |
| Urutan permintaan | Respons lambat menimpa respons baru | Hanya `requestId` yang sedang berlaku boleh menulis |
| **Versi folio** | Balasan replika baca yang tertinggal | Folio dengan `version` lebih kecil ditolak |

Lapis pertama saja tidak cukup: dua permintaan dapat selesai berurutan dengan benar dan tetap
membawa data yang lebih lama. Penanda permintaan **sengaja tidak dikosongkan** setelah selesai —
memperlakukan penanda kosong sebagai "boleh lewat" akan meloloskan respons tertinggal, justru yang
hendak dicegah. Ada test yang mengunci kedua perilaku itu.

### 4.4 Nominal kosong bukan nol rupiah

`grossAmount` dan `eligibleAmount` boleh `null`, dan `null` berarti **belum dihitung** — bukan
**tidak ada biaya**. Menyamakan keduanya akan menampilkan total yang lebih kecil daripada tagihan
sebenarnya.

Karena itu baris yang belum dihitung tidak ikut dijumlahkan, ditampilkan sebagai *"Belum dihitung"*,
dan totalnya diberi peringatan: *"Angka ini belum boleh dipakai sebagai tagihan akhir."*

---

## 5. Bukti acceptance criteria

| # | Kriteria | Cara dipenuhi | Bukti test |
| --- | --- | --- | --- |
| 1 | `OutcomeUnknown` bukan failed dan bukan success | Nada `belum-pasti` tersendiri; kalimat terkunci *"Hasil pemrosesan belum dapat dipastikan"* | 6 test |
| 2 | `404` sebagai tidak ditemukan, bukan kosong | `classifyHttpFailure` memisahkan `notFound`; folio kosong tetap objek, ketiadaan folio bernilai `null`; layar menampilkan judul *"Folio tidak ditemukan"*, bukan tabel kosong | 2 test |
| 3 | `409` menampilkan konflik beserta reload terkontrol | Jenis `conflict` tersendiri; kartu konflik dengan tombol **Muat ulang folio** | 1 test |
| 4 | UUID bukan satu-satunya label di layar | `buildChargeLineTitle` selalu menyusun dari konteks sumber dan jenis efek; UUID hanya tampil sebagai *"Rujukan"* pendamping | 4 test |

Kalimat wajib `PendingFinancialReview` → *"menunggu tinjauan finansial"* juga dikunci test.

`401` dan `403` dibedakan dari kegagalan umum dan ditampilkan sebagai akses ditolak, tidak pernah
disamarkan sebagai data kosong.

---

## 6. Validasi yang dijalankan

| Validasi | Perintah | Hasil |
| --- | --- | --- |
| Test unit baru | `node --test tests/unit/billing-folio-utils.test.mjs` | **29 lulus, 0 gagal** |
| Seluruh test unit | `npm run test:unit` | **67 lulus, 0 gagal** (38 lama + 29 baru) |
| Lint berkas Billing | `npx eslint <berkas billing>` | **0 masalah** |
| Build | `npm run build` | **Lulus**, keluar dengan kode `0` |

### 6.1 Hasil build

`npm run build` selesai dengan **exit code `0`**, dilanjutkan `postbuild` yang melaporkan
*"Standalone runtime siap dijalankan."* Tidak ada satu pun error kompilasi.

Route baru terbukti benar-benar dihasilkan, bukan sekadar tidak menggagalkan build:

```
.next/app-path-routes-manifest.json
  /health-services/billing-management/billing-folio/[encounterId]/page

.next/server/app/health-services/billing-management/billing-folio/[encounterId]
```

Route bersifat dinamis (`ƒ`), sebagaimana mestinya: `encounterId` baru diketahui saat permintaan
tiba, sehingga tidak ada yang dapat diprarender.

### 6.2 `MANUAL TEST: NOT FEASIBLE`

Verifikasi interaktif manual **tidak dapat dijalankan** dari sesi ini, dengan alasan konkret:

1. **Tidak ada data folio yang dapat dijangkau.** Backend `RJ-BIL-BE-001` berjalan terhadap
   `QuilvianNewDevTim01`, dan folio hanya lahir dari milestone klinis. Tidak ada satu pun folio
   yang diketahui ada di sana di luar yang dibuat test integrasi lalu dibersihkan kembali.
2. **Layar belum masuk navigasi mana pun.** Wewenang menempatkan menu ada pada `FRONTEND_AUTHORITY`
   yang masih `OPEN`, sehingga route-nya hanya dapat dicapai dengan mengetik URL langsung.
3. **Sesi ini tidak menjalankan browser.** Menjalankan `next dev` dan menekan kendali satu per satu
   berada di luar yang dapat dibuktikan dari sini.

Yang **dapat** dibuktikan sudah dibuktikan: seluruh logika keputusan — pembedaan `404`/`403`/`409`,
nada hasil pemrosesan, penjaga respons usang, dan perlakuan nominal kosong — berada di utility
murni dan diuji langsung. Yang **tidak** dapat dibuktikan dari sini adalah render dan interaksinya.

Harness test project ini adalah `node --test` tanpa `@testing-library`, sehingga **component render
test memang belum mungkin** ditulis. Ini bukan pilihan; ini batas alat yang ada. Menutupnya adalah
cakupan `RJ-BIL-FE-007`.

---

## 7. Yang sengaja tidak dikerjakan

| Hal | Alasan |
| --- | --- |
| Menu dan penempatan navigasi | `FRONTEND_AUTHORITY` masih `OPEN`. Menempatkan menu sepihak berarti mengambil wewenang orang lain |
| Menyamakan SHA frontend pada governance | Wewenang pemilik frontend; lihat bagian 1 |
| Melengkapi baris Dependency roadmap dengan `BE-007` | Wewenang pemilik roadmap; dicatat sebagai delta di bagian 2 |
| Component render test | Harness belum mendukung; cakupan `RJ-BIL-FE-007` |
| Layar Radiologi | `RJ-BIL-BE-004` ⛔ terblokir |
| Satu pun kendali yang mengubah nilai finansial | Dilarang Definition of Done task ini dan `RJ-BIL-GATE-DEC-001` |

---

## 8. Risiko yang diketahui

1. **Status pemrosesan dimuat per baris, bukan sekaligus.** Folio dengan puluhan baris akan
   melahirkan puluhan permintaan bila dimuat di muka, karena `processing-status` hanya menerima satu
   identitas sumber per panggilan. Karena itu ia dimuat **hanya ketika satu baris dibuka**.
   Konsekuensinya: petugas tidak melihat hasil pemrosesan seluruh baris sekaligus. Bila kelak itu
   dibutuhkan, yang benar adalah endpoint kolektif di backend — bukan kipas permintaan dari layar.

2. **`BilFolio.Status` dapat basi terhadap baris tagihannya.** Sudah dicatat pada laporan
   `RJ-BIL-BE-006` bagian 8 dan **belum diperbaiki**; kolom itu milik `RJ-BIL-BE-001`. Layar ini
   menampilkan status folio apa adanya, sehingga bila kolomnya basi, layarnya ikut menampilkan yang
   basi. Yang tidak ikut basi adalah baris tagihannya, karena itu dibaca langsung.

3. **Total layak tagih bukan tagihan akhir.** Ia belum melalui alokasi penanggung (`RJ-BIL-BE-005`,
   ⛔ terblokir). Layar menyatakannya, tetapi angka di layar tetap berpotensi dibaca sebagai final
   oleh petugas yang terburu-buru.

---

## 9. Yang masih terbuka

| Hal | Pemilik |
| --- | --- |
| `FRONTEND_AUTHORITY` — route final, menu, bentuk komponen | Frontend authority, masih `OPEN` |
| `SECURITY_PRIVACY_SIGNOFF` | Security/Privacy, masih `OPEN` |
| Baris Dependency `FE-001` belum menyebut `RJ-BIL-BE-007` | Pemilik roadmap |
| SHA frontend pada manifest dan `MODULE-STATUS.md` | Pemilik frontend |

---

## 10. Batas yang dipatuhi

`commit` / `push` / `merge` / `rebase` / `deploy`: **TIDAK**. Perubahan backend: **TIDAK**.
Aktivasi `RJ-BIL-DEP-009`: **TIDAK**. Mutasi finansial dari frontend: **TIDAK ADA SATU PUN**.

Working tree frontend hanya memuat berkas Billing yang baru dan satu baris registrasi reducer pada
`store.jsx`. Tidak ada perubahan pengguna yang tidak terkait ikut tersentuh.
