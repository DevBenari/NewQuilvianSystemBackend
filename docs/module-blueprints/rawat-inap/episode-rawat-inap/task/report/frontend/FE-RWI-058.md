# Laporan Perubahan Frontend — `FE-RWI-058`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-058` |
| Judul | Langkah Deposit berdiri di antara Pembayaran dan Dokter |
| Slice | `F14` — Deposit pada alur admisi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), kartu `FE-RWI-058` |
| Trace | `RWI-DEC-093`; `RWI-DEC-075` sebagaimana diamandemen; `FR-RI-174`; `03-frontend-architecture.md` `0.6` bagian 3A.2 dan 3A.3 |
| Skema tampilan | **`FE-INP-20`, ditulis 12 September 2026** pada `05-skema-tampilan.md` bagian 3.5A, revision naik ke `0.5`. Inilah yang menutup `RWI-UI-GAP-008`. Skemanya berstatus `draft` dan **belum disetujui pemilik** |
| Wewenang UI | Tata letak isian, penempatan keterangan, dan bentuk indikator langkah `DEV_DISCRETION`, dengan batas: nominal wajib terbaca sebagai rupiah, dan tombol lanjut **tidak boleh** terkunci oleh nilai apa pun. Kedua batas itu ditaati |
| Klasifikasi | `MEDIUM` — repository 1, berkas baru 4, berkas diubah 3, logika bisnis 0, kontrak API **0**, database 0 |
| Task mode | `FRONTEND` |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `7f6b9356f6349d516570d6d603ca026f2c7f4ec2` pada branch `HamzahV2` — perubahan masih lokal, belum di-commit |
| Tanggal | 12 September 2026 |
| Status | ✅ **Selesai 12 September 2026.** Kelima acceptance criteria terpetakan ke source dan dikunci delapan test baru. `npm run test:unit` **763 lulus, 0 gagal**; `npm run lint:errors` **0 error**; `npm run build` beserta `postbuild` berhasil. **Satu selisih antar-dokumen dicatat, bukan ditutup diam-diam** — lihat bagian 5. Bukti peramban **`NOT RUN`**: repository tidak punya `playwright.config.*` |

---

## 1. Keadaan yang ditemukan di awal

Alur admisi rawat inap tidak punya tempat untuk mencatat uang muka. Petugas menerima uang
dari keluarga pasien di meja admisi, tetapi satu-satunya cara mencatatnya adalah membuka
layar Billing terpisah sesudah episode terbentuk.

`RWI-DEC-093` memutuskan langkah Deposit disisipkan **di antara Pembayaran dan Dokter**,
karena itulah urutan kerja rumah sakit hari ini: uang muka diterima sesudah cara bayar dan
kelas ditetapkan, sebelum DPJP dipilih.

**Task ini tertahan `RWI-UI-GAP-008`, bukan tertahan backend.** Gapnya: `05-skema-tampilan.md`
revision `0.4` hanya memuat `FE-INP-01` s.d. `FE-INP-19`, dan skema langkah Deposit belum
pernah ditulis. Roadmap menyatakan penyusunan skemanya **pekerjaan desain**, bukan wewenang
roadmap.

---

## 2. Yang dikerjakan lebih dulu — menutup `RWI-UI-GAP-008`

`05-skema-tampilan.md` bagian **3.5A** ditulis, memuat `FE-INP-20`: kerangka layar, tabel
wilayah, tabel tombol, enam aturan yang mengikat, tabel keadaan, dan batas layar.

Revision dokumen naik `0.4` → `0.5`. **Kesembilan belas layar lain tidak diubah satu baris
pun**, dan statusnya tetap `draft` menunggu persetujuan pemilik. Bagian 3.5A berkata demikian
apa adanya: ia disusun atas permintaan pemilik untuk membuka `FE-RWI-058` dan `FE-RWI-060`,
dan **belum disetujui**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas baru

| Berkas | Isi |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-admission-deposit-storage.jsx` | Penyimpanan nominal di `sessionStorage`, pola disalin dari penyimpanan langkah Pembayaran |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx` | Controller langkah. **Nol endpoint dipanggil** |
| `src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx` | Layar langkah |
| `tests/unit/inpatient-admission-deposit.test.mjs` | Delapan test |

### 3.2 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `…/inpatient-admission-flow-constants.jsx` | `INPATIENT_DEPOSIT_STEP_SLUG`; langkah disisipkan pada **kedua** daftar; penomoran digeser; `INPATIENT_DEPOSIT_STEP_TEXT` |
| `…/inpatient-admission-view.jsx` | Hook dipasang di level view; cabang render langkah |
| `…/inpatient-admission.module.css` | Lima kelas isian nominal, seluruhnya memakai token yang sudah dipakai stylesheet ini |

### 3.3 Tiga keputusan teknis yang perlu diketahui

**Nominal disimpan sebagai teks digit, bukan sebagai angka.** `JSON.parse` mengembalikan
`number`, dan nominal rupiah yang melewati `number` kehilangan ketepatannya pada nominal
besar. Isian ini berakhir sebagai `decimal` di server, jadi ia diperlakukan sebagai teks
sepanjang hidupnya di layar.

**Penyimpanan memakai `sessionStorage`, bukan `localStorage`.** Nominal ini milik satu tab
yang sedang mengerjakan satu admisi. `localStorage` akan membocorkannya ke tab lain yang
mungkin sedang melayani pasien berbeda.

**Tujuan pelanjutan admisi tidak disentuh, dan memang tidak perlu.**
`INPATIENT_ADMISSION_RESUME_STEPS` menyebut tujuannya lewat **slug**, bukan angka, sehingga
pergeseran penomoran tidak merusak `FE-RWI-032`. Itu diperiksa test tersendiri, bukan
diasumsikan.

---

## 4. Verifikasi

| Butir | Hasil |
| --- | --- |
| `npm run test:unit` | **763 lulus, 0 gagal** — naik dari 755; delapan test baru |
| `npm run lint:errors` | **0 error** |
| `npm run build` | **✓ Compiled successfully**, `postbuild` berhasil |
| `FE-RWI-032` mendarat pada langkah yang benar | ✅ dikunci test; tujuan pelanjutan disebut lewat slug dan kedua slug terbukti masih ada pada kedua jalur |
| E2E kedua jalur di Edge | **`NOT RUN`** — repository tidak memiliki `playwright.config.*` |
| Devtools network kosong selama langkah ini | **`NOT RUN` sebagai pemeriksaan peramban**, tetapi digantikan test source: nol `InstanceAxios`, nol `.service`, nol `fetch(`, dan nol metode HTTP pada kedua berkas langkah |

---

## 5. Acceptance criteria — satu selisih dicatat apa adanya

| # | Kriteria | Hasil |
| --- | --- | --- |
| 1 | Jalur pasien lama memuat **sembilan** langkah dengan Deposit pada urutan **keempat** | ⚠️ **Selisih. Lihat di bawah** |
| 2 | Jalur pasien baru memuat sepuluh langkah dengan Deposit pada urutan keempat | ✅ tepat |
| 3 | Nominal bertahan saat mundur ke Pembayaran lalu maju lagi | ✅ |
| 4 | Tidak ada permintaan jaringan dikirim dari langkah ini | ✅ |
| 5 | Memuat ulang halaman tidak mengembalikan petugas ke langkah pertama | ✅ |

**Selisih kriteria 1, dan kenapa angkanya tidak dipaksakan.**

Kriteria 1 menuntut jalur pasien lama **sembilan langkah dengan Deposit di urutan keempat**.
Source per 12 September 2026 tidak dapat memenuhi angka itu:

| | Sebelum task ini | Sesudah |
| --- | :---: | :---: |
| Jalur pasien lama | **9 langkah**, Deposit belum ada | **10 langkah**, Deposit di urutan **5** |

Jalur pasien lama sudah sembilan langkah **sebelum** Deposit disisipkan, karena langkah
`Informasi Pasien Lama` ada di sana. Angka pada kartu roadmap disusun ketika jalur itu masih
delapan langkah. `03-frontend-architecture.md` bagian 3A.3 memperlihatkan ketidakcocokan yang
sama: barisnya menulis rentang "4–9" untuk **tujuh** butir.

Memenuhi angka sembilan menuntut **menghapus satu langkah yang benar-benar dipakai**, dan itu
di luar scope task ini serta merusak jalur pasien lama.

Yang mengikat dan dipenuhi adalah **posisinya**: Deposit berdiri tepat di antara Pembayaran
dan Dokter pada kedua jalur — itu judul task ini, `RWI-DEC-093`, dan bagian 3A.2. Kedua hal
itu dikunci test.

**Yang dibutuhkan pemilik:** membetulkan angka pada kartu `FE-RWI-058` dan pada
`03-frontend-architecture.md` bagian 3A.3 menjadi **sepuluh langkah, Deposit pada urutan
kelima** untuk jalur pasien lama.

---

## 6. Catatan penutup

**Layar ini sengaja belum menampilkan minimum kebijakan.** Pembacaan
`GET /deposit-policies` adalah pekerjaan `FE-RWI-059`, dan endpoint itu **nol barisnya ada**
di backend per 12 September 2026 — `BE-BKC-039` masih `BLOCKED_PENDING_OWNER_APPROVAL`.
Alih-alih menampilkan angka minimum yang dikarang, layar mengaku minimumnya belum dapat
dibaca dan menegaskan admisi tidak tertahan. Nol angka minimum ditulis di kode layar, dan
itu dikunci test.

**Skema `FE-INP-20` belum disetujui.** Ia ditulis untuk membuka task ini atas permintaan
pemilik, dan statusnya tetap `draft`. Persetujuan pemilik tetap dibutuhkan sebelum bentuk
layarnya dianggap terkunci.
