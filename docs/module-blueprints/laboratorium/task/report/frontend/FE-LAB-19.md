# Laporan Perubahan Frontend — `FE-LAB-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-19` |
| Judul | Perbaikan: layar wadah mengirim jenis specimen |
| Slice | `S2` — perbaikan atas `FE-LAB-07` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6d |
| Trace | `LAB-DEC-040`, `LAB-DEC-041`, `LAB-DEC-042`, `BR-35`; `VAL-51`, `VAL-53`, `VAL-56`, `VAL-58` |
| Contract version | `LAB-API-v1` `r7` grup Lab Specimen — `approved`, terkunci. **Nol amandemen** |
| Wewenang UI | Ruas menyusul pola dialog rencana yang sudah ada; nol pola baru |
| Dependency | `FE-LAB-11` ✅ — aturan murninya berasal dari sana |
| Klasifikasi | `MEDIUM` — perbaikan terpusat, tetapi menyentuh aturan murni yang dipakai dua layar |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `686038858` |
| Commit backend yang dijadikan rujukan | `13665452` |
| Tanggal | 2026-09-17 |
| Status | **`SELESAI`** — cacat tertutup, dan satu uji lama yang mengunci kontrak cacat itu **sengaja dibalik**; lihat bagian 4.1 |

---

## 1. Cacat yang diperbaiki

**Layar wadah tidak lagi dapat merencanakan wadah sama sekali.**

`BE-LAB-21` menambahkan lima ruas bahan wadah lewat `LAB-API-v1` `r7`, dan `VAL-51` menolak
**tanpa syarat** setiap wadah baru yang tidak membawa `SpecimenTypeId`:

```csharp
if (specimenTypeId == Guid.Empty)
    throw new LabSpecimenValidationException("Pilih jenis specimen terlebih dahulu.");
```

Sementara itu `buildPlanPayload` hanya membawa dua ruas:

```js
export const buildPlanPayload = (form = {}) => ({
  examinations: (...).map(String),
  specimenDescription: String(form.specimenDescription ?? "").trim() || null,
});
```

Akibatnya **setiap** perencanaan wadah dari layar pesanan dijawab `422`. Berlaku sejak migration
`BE-LAB-21` diterapkan pada **2026-09-15** — dua hari sebelum task ini.

### 1.1 Kenapa ia tidak tertangkap apa pun

Ini yang pantas dibaca dua kali. Cacatnya **tidak terlihat** oleh:

| Penjaga | Kenapa lolos |
| --- | --- |
| Build | Payload-nya JavaScript biasa; nol ruas yang hilang berarti nol galat kompilasi |
| Uji unit | Uji yang ada justru **mengunci perilaku cacatnya** — `validatePlan({ examinations: ["a"] })` diharapkan mengembalikan `{}` |
| Lint | Nol kaitannya |
| Tinjauan kontrak | `r7` memang sudah disetujui; yang tidak pernah dikerjakan adalah sisi frontend-nya |

Yang menemukannya adalah **penelusuran kelima nama ruas pada seluruh `src`** saat `FE-LAB-11`
dikerjakan — dan hasilnya nol kemunculan. `traceability.md` revisi 18 bahkan sudah meramalkannya
pada 2026-09-15; yang belum pernah dikerjakan adalah layar lamanya.

---

## 2. Proses bisnis dari sisi pengguna

Petugas membuka pesanan laboratorium, menekan **Rencanakan Wadah**, lalu:

1. Memilih pemeriksaan yang akan dikerjakan dari wadah itu — **seperti sebelumnya**.
2. **Baru:** memilih **Jenis Specimen** dari daftar. Bila yang dipilih `Lainnya`, satu kotak
   keterangan muncul dan wajib diisi.
3. **Baru:** mengisi **Volume** beserta satuannya — boleh dikosongkan.
4. **Baru:** mengisi **Waktu Penerimaan Fisik** bila sampel tiba pada waktu yang berbeda dari
   waktu pencatatan.
5. Menekan simpan. **Permintaannya kini diterima**; sebelumnya selalu ditolak.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/.../lab-specimen-rules.js` | `validatePlan` menerima daftar pilihan jenis dan **mendelegasikan** pemeriksaan bahan; `buildPlanPayload` meneruskan kelima ruas `r7` |
| `src/lib/hooks/.../use-lab-specimen-workspace.jsx` | `planForm` memuat ruas bahan sejak awal; daftar pilihan jenis dimuat; `volumeUnitSelect` disediakan |
| `src/components/view/.../lab-specimen-workspace-view.jsx` | Empat ruas dirender pada dialog rencana; kotak keterangan hanya muncul untuk `Lainnya` |
| `tests/unit/lab-specimen-rules.test.mjs` | **+4 uji baru**, dan **1 uji lama dibalik** — lihat 4.1 |

### 3.1 Aturan murninya **dipakai ulang, bukan disalin**

`validatePlan` memanggil `validateMaterialForm` dan `validatePhysicalReceipt` dari
`lab-reception-rules.js` — berkas yang dibuat `FE-LAB-11`. `buildPlanPayload` memanggil
`buildSpecimenPlanPayload` dari tempat yang sama.

**`VAL-51`, `VAL-53`, dan `VAL-56` karena itu punya satu definisi di seluruh modul.** Menyalinnya
akan melahirkan definisi kedua — dan salinan itulah yang kelak bercabang, sehingga kedua layar
mulai menolak hal yang berbeda tanpa ada yang menyadarinya.

### 3.2 Satu keputusan yang pantas dibaca ulang

**Daftar jenis yang gagal dimuat TIDAK menahan petugas.** Ketika daftarnya belum diketahui,
pemeriksaan `Lainnya` dilewati dan backend yang menegakkannya.

Menahan petugas karena daftar yang belum tiba adalah kegagalan yang lebih buruk daripada satu
penolakan `422` yang terbaca: yang pertama menghentikan pekerjaan tanpa sebab yang terlihat, yang
kedua memberi kalimat yang dapat dilaporkan.

---

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `npx eslint --quiet` pada berkas yang disentuh | Nol keluaran — **`PASS`** |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | **1033/1033 lulus** — **`PASS`** |
| `npm run build` | Lulus — **`PASS`** |
| `npx playwright test` — **seluruh 4 spec Laboratorium** | **18 passed** — **`PASS`** |

### 4.1 Satu uji lama sengaja dibalik, dan itu bukan regresi

Uji `FE-LAB-07 (VAL-05)` semula berbunyi:

```js
assert.deepEqual(validatePlan({ examinations: ["a"] }), {});
```

Ia **mengunci perilaku yang cacat** — menyatakan bahwa rencana wadah tanpa jenis specimen adalah
sah, padahal backend menolaknya. Uji itu kini menuntut jenis specimen, dan satu uji baru
ditambahkan yang memeriksa penolakannya secara langsung:

```js
const errors = validatePlan({ examinations: ["a"] }, OPSI_JENIS);
assert.equal(errors.specimenTypeId, "Pilih jenis specimen terlebih dahulu.");
```

**Perubahan perilaku yang disengaja**, dicatat di sini supaya tidak terbaca sebagai uji yang
"diperbaiki agar lulus".

### 4.2 Empat uji baru

| Uji | Yang dibuktikan |
| --- | --- |
| `VAL-51` | Rencana tanpa jenis specimen **ditolak di layar**, bukan baru di backend |
| `VAL-53` | Jenis `Lainnya` menuntut keterangan |
| Daftar belum termuat | **Tidak** menahan petugas — pembuktian terbalik atas 3.2 |
| `buildPlanPayload` | Muatannya **kini membawa** `specimenTypeId`, `volumeAmount`, dan `volumeUnitId`; dan **tetap nol** membawa `quantity` (`LAB-DEC-050`) |

### 4.3 Batas yang disebut apa adanya

`MANUAL TEST: NOT FEASIBLE` untuk jalur ujung-ke-ujung terhadap backend sungguhan. Membuktikannya
menuntut pesanan laboratorium beserta pemeriksaan yang belum berwadah pada basis data bersama,
dan membuat data uji untuk itu berada di luar wewenang task frontend.

**Yang dibuktikan sebagai gantinya** adalah bahwa muatannya kini membawa ruas yang persis
dituntut `VAL-51`, diperiksa dari source backend dan dikunci uji unit. Penjaga layarnya pun
memakai kalimat yang **sama persis** dengan backend.

---

## 5. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Perencanaan wadah berhasil kembali | **Terpenuhi pada muatan** | 4.2; batas pembuktiannya pada 4.3 |
| Jenis `Lainnya` menuntut keterangan | **Terpenuhi** | 4.2 |
| Volume tanpa satuan tertolak di layar | **Terpenuhi** | Diwarisi `validateMaterialForm`, teruji `FE-LAB-11` S5 |
| Aturan murninya **dipakai ulang, bukan disalin** | **Terpenuhi** | 3.1 |
| Seluruh uji lama lulus | **Terpenuhi, dengan satu pengecualian yang disengaja** | 1033/1033; satu uji dibalik — 4.1 |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning baru |
| Masalah yang diketahui | Pembuktian ujung-ke-ujung menunggu data pesanan; lihat 4.3 |
| Dependency | `NONE` |
| Perubahan sampingan | `NONE`. `test-results/` dihapus sesudah pemeriksaan |
| Interupsi | `NONE` |
| Status Git | Nol `git add`, `commit`, maupun `push` |
| Langkah berikutnya | `FE-LAB-12` — daftar dan detail penerimaan, satu-satunya sisa `MVP-5a` |
