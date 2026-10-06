# ISSUE-003 — Langkah 2 Pendaftaran Pasien Baru: tombol tanpa fungsi, isian UUID, label kota kembar, dan pilihan yang saling bertentangan

```yaml
issue_id: ISSUE-EPS-003
module_id: rawat-inap
submodule: episode-rawat-inap
layar: "Admisi Rawat Inap — Langkah 2 Pendaftaran Pasien Baru (FE-INP-03, skema tampilan 3.3)"
sumber_laporan: "Laporan pemilik 06-10-2026: 7 butir bernomor (butir 3 kosong), 8 lampiran screenshot dan capture database"
tanggal_issue: "2026-10-06"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "00fc141f23a6cf04862235dfd3876be22e581b8a (MHamzah)"
source_sha_frontend: "b010ffb9722f236160477c95c931c22467550200 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-003-pendaftaran-pasien-baru.md
ditulis_dengan: "skill diagnose-module-issue (penggunaan pertama)"
```

## 1. Ringkasan

Laporan ini menyoroti tujuh hal pada Langkah 2 alur admisi rawat inap, yaitu formulir untuk
mendaftarkan pasien yang belum pernah terdaftar. Enam butir berhasil dianalisis sampai ke baris
source; satu butir (nomor 3) kosong pada laporan dan perlu dikonfirmasi pelapor.

Sebagian besar masalah berakar sama: formulir ini **memakai ulang komponen pendaftaran IGD apa
adanya**, termasuk bagian-bagian yang tidak pernah dirancang untuk admisi rawat inap — tombol
"Input Manual", bagian "Data tambahan pasien", dan isian berbasis UUID.

Dua hal paling berbahaya: **isian UUID** yang mustahil diisi petugas (salah satunya bahkan pasti
ditolak server), dan **dua pilihan "Bekasi"** yang tidak bisa dibedakan — ditambah temuan bahwa hasil
scan KTP "Kota Bekasi" dapat diam-diam terpasang ke "Kabupaten Bekasi".

Satu butir — tombol Simpan yang mati — ternyata **sesuai** skema tampilan yang disetujui. Butir itu
dicatat sebagai perubahan desain atas permintaan pemilik, bukan bug.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (diringkas) | Lampiran |
| :---: | --- | --- |
| 1 | Tombol "Input Manual" untuk apa? Hilangkan saja, tidak berguna — formulir di bawah sudah bisa diisi; untuk scan cukup Cek Scanner lalu Scan eKTP | Lampiran 2 |
| 2 | Pilihan "Bekasi" terlihat ganda, padahal itu Kabupaten dan Kota. Gunakan `CityType` | Lampiran 5 (daftar pilihan), lampiran 6 (capture `MstCity`) |
| 3 | *(kosong)* | — |
| 4 | Kenapa menginput UUID? Aturan: (a) UUID tidak boleh diinput user; (b) UUID tidak boleh tampil di halaman frontend | Lampiran 7, lampiran 1 |
| 5 | Jam lahir gunakan base component | — |
| 6 | Checkbox pada Data tambahan pasien kenapa bisa diklik semua? Minta rekomendasi UI/UX yang baik | Lampiran 1 |
| 7 | Tombol "Simpan & Lanjut ke Pembayaran" jangan dimatikan; tetap bisa diklik, lalu bila ada isian wajib yang kosong, arahkan ke isian tersebut | Lampiran 8 |

Lampiran yang **tidak dirujuk** butir mana pun secara langsung: **lampiran 3** (Identitas Pasien serta
Dokumen dan Kontak) dan **lampiran 4** (Alamat Pasien, Data tambahan tertutup, Kontak Darurat, dan
tombol aksi). Keduanya mungkin milik butir 3 — lihat pertanyaan P-01.

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-003-01` | 1 | Tombol "Input Manual" tidak berbuat apa-apa | `BUG` | FE | Medium | SUDAH-VERIFIKASI | `FIX-EPS-003-01` |
| `ISS-EPS-003-02` | 2 | Dua pilihan "Bekasi" tanpa keterangan Kota/Kabupaten | `CONTRACT_MISMATCH` | FE | High | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-003-02` |
| `ISS-EPS-003-03` | 3 | Butir kosong | `PERLU_KONFIRMASI` | — | — | — | — |
| `ISS-EPS-003-04` | 4 | Petugas diminta mengetik UUID | `RULE_VIOLATION`, `BUG` | FE | High | SUDAH-VERIFIKASI | `FIX-EPS-003-04` |
| `ISS-EPS-003-05` | 5 | Jam Lahir memakai kontrol bawaan peramban | `BUG` | FE | Low | SUDAH-VERIFIKASI | `FIX-EPS-003-05` |
| `ISS-EPS-003-06` | 6 | Member, Bayi Baru Lahir, dan Meninggal dapat dicentang bersamaan | `BUG` | FE | High | SUDAH-VERIFIKASI | `FIX-EPS-003-06` |
| `ISS-EPS-003-07` | 7 | Tombol Simpan mati tanpa memberi tahu isian yang kurang | `DESIGN_CHANGE` | FE + dokumen | Medium | SUDAH-VERIFIKASI | `FIX-EPS-003-07`, `FIX-EPS-003-10` |
| `ISS-EPS-003-T1` | — | Hasil scan KTP "Kota Bekasi" dapat terpasang ke "Kabupaten Bekasi" | `BUG` | FE | High | SUDAH-VERIFIKASI (kode); `DUGAAN` (urutan data) | `FIX-EPS-003-03` |
| `ISS-EPS-003-T2` | — | Pesan galat scanner tampil mentah "Failed to fetch" | `BUG` | FE | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-003-08` |
| `ISS-EPS-003-T3` | — | Isian "Metode Persalinan" tampil untuk semua pasien | `BUG` | FE | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-EPS-003-09` |
| `ISS-EPS-003-T4` | — | Daftar pilihan Tier Membership di frontend menunjuk rute yang tidak ada | `CONTRACT_MISMATCH` | FE + BE | Medium | SUDAH-VERIFIKASI | di luar rencana — modul pasien |
| `ISS-EPS-003-T5` | — | Master data pasien juga meminta UUID "Membership Aktif" | `RULE_VIOLATION` | FE | Medium | SUDAH-VERIFIKASI | di luar rencana — modul pasien |

Radius dampak umum: `new-patient-form.jsx`, `emergency-registration-fields.jsx`, dan
`emergency-region.service.js` milik **pendaftaran IGD** (`registration-management`) dan dipakai bersama
oleh admisi rawat inap. Pemakai `new-patient-form` hanya dua: `patient-selection-step.jsx` (IGD) dan
`inpatient-admission-registration-step.jsx` (rawat inap). `plustek-scan-panel.jsx` hanya dipakai rawat
inap.

---

## 4. Rincian per temuan

### ISS-EPS-003-01 — Tombol "Input Manual" tidak berbuat apa-apa

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` — menyimpang dari skema tampilan 3.3 |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-003-01` |

**Apa yang terjadi.** Saat pemindai belum siap, panel scan menampilkan tombol "Input Manual". Petugas
yang menekannya tidak mendapat reaksi apa pun — tidak ada formulir yang terbuka, tidak ada halaman yang
bergulir. Formulir pasien sendiri sudah tampil penuh tepat di bawah panel.

**Kenapa terjadi.** Panel menampilkan tombol setiap kali pemindai bermasalah, lalu memanggil
`onUseManual` saat ditekan — `plustek-scan-panel.jsx:38-40` dan `:130-139`:

```javascript
const shouldShowManualAction = Boolean(
  !online || !ocrReady || scannerError || (previewUrl && !hasIdentityData),
);
// …
{shouldShowManualAction ? (
  <button type="button" className={styles.secondaryButton}
    onClick={onUseManual} disabled={scanning || manualEntryOpen}>
    {manualEntryOpen ? "Form Manual Terbuka" : "Input Manual"}
  </button>
) : null}
```

Admisi rawat inap tidak pernah mengirim `onUseManual` maupun `manualEntryOpen` —
`inpatient-admission-registration-step.jsx:87-96` hanya mengirim `scanner`, `onScan`,
`onRefreshStatus`, dan `onClearResult`. Akibatnya `onClick` bernilai `undefined`.

Panel itu sendiri tidak konsisten: tombol "Cek Scanner" dan "Hapus Hasil" hanya dirender bila
handler-nya ada (`typeof onRefreshStatus === "function"`, baris 141; `typeof onClearResult ===
"function"`, baris 156), tetapi tombol "Input Manual" tidak diberi penjaga yang sama.

**Akar.** Tombol ini lahir untuk pola IGD, tempat formulir pasien baru **disembunyikan** sampai petugas
memilih isi manual — lihat `patient-selection-step.jsx:1518` (`{manualEntryOpen ? (` membungkus
formulir). Di admisi rawat inap formulirnya **selalu tampil**, sehingga tombol itu tidak punya tugas.

**Apakah ini menyimpang dari desain?** Ya. Skema tampilan 3.3 (`05-skema-tampilan.md:243-246`) hanya
memuat tombol "Mulai Scan KTP" dan kalimat *"Pemindai tidak tersedia? Isi formulir di bawah secara
manual."* — tidak ada tombol Input Manual. Bagian Keadaan (baris 280) mewajibkan kalimat penjelas itu
tampil saat pemindai tidak tersedia; kalimat tersebut justru belum ada pada panel sekarang.

**Dampak nyata.** Petugas admisi yang pemindainya mati menekan "Input Manual", tidak terjadi apa-apa,
lalu menyangka formulir terkunci dan menelepon bagian IT, padahal formulirnya bisa langsung diisi.

**Rekomendasi.** Tampilkan tombol hanya bila pemakai panel memberi handler `onUseManual`, sama seperti
dua tombol lainnya, dan tambahkan kalimat penjelas sesuai skema 3.3. Rincian di `FIX-EPS-003-01`.

---

### ISS-EPS-003-02 — Dua pilihan "Bekasi" tanpa keterangan Kota/Kabupaten

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `CONTRACT_MISMATCH` — frontend membaca nama field yang tidak dikirim backend |
| **Area** | Frontend |
| **Keparahan** | High — membuka peluang alamat tersimpan pada wilayah yang salah |
| **Status bukti** | SUDAH-VERIFIKASI di source; data dari capture pelapor (lampiran 6) |
| **Perbaikan** | `FIX-EPS-003-02` |

**Apa yang terjadi.** Pada provinsi Jawa Barat, daftar Kota/Kabupaten memuat dua baris bertuliskan
sama persis: "Bekasi" dan "Bekasi". Petugas tidak dapat mengetahui mana Kota Bekasi dan mana Kabupaten
Bekasi. Setelah dipilih pun isiannya hanya menampilkan "Bekasi".

**Datanya benar, bukan data ganda.** Capture `MstCity` (lampiran 6) memperlihatkan dua baris berbeda pada
provinsi yang sama:

| `CityCode` | `CityName` | `CityType` |
| --- | --- | --- |
| `32.16` | Bekasi | Kabupaten |
| `32.75` | Bekasi | Kota |

**Kenapa terjadi.** Backend **sudah mengirim** jenis kota, tetapi lewat field bernama `additionalInfo`.
`RegionController.cs:1617-1628`, endpoint `GET /api/v1/administrator/master-data/regions/cities/options`:

```csharp
.Select(x => new RegionOptionResponse
{
    Id = x.Id,
    Code = x.CityCode,
    Name = x.CityName,
    // …
    AdditionalInfo = x.CityType,
```

`RegionOptionResponse` (`RegionDtos.cs:152-163`) memang tidak punya field `CityType`; satu-satunya
tempat jenis kota adalah `AdditionalInfo`.

Frontend justru mencari field lain — `emergency-region.service.js:109-115`:

```javascript
const cityType = toSafeString(item.cityType || item.cityTypeName);

let label = name || "-";

if (type === "city" && cityType && name) {
  label = `${cityType} ${name}`;
}
```

`cityType` dan `cityTypeName` tidak pernah ada pada respons, sehingga `cityType` selalu kosong dan label
jatuh ke nama saja. Logika penggabung "Kota Bekasi" sebenarnya sudah ditulis; ia hanya tidak pernah
mendapat data.

**Akar.** Logika pencarian wilayah ditulis dua kali dan menyimpang. Kiosk pendaftaran sudah membaca field
yang benar — `kiosk-new-patient-registration.helpers.jsx:536-544`:

```javascript
export const getRegionCityTypeText = (option) =>
  normalizeTextValue(
    option?.cityType ||
      option?.typeName ||
      option?.regionType ||
      option?.additionalInfo ||
      inferRegionCityType(option?.cityName || option?.name || "") ||
      "",
  );
```

Service milik IGD yang dipakai admisi rawat inap ditulis terpisah, menebak nama field, dan tidak pernah
dicocokkan dengan kontrak backend.

**Apakah ini menyimpang dari desain?** Ya — kontrak backend sudah menyediakan jenis kota; frontend tidak
memakainya. Tidak ada perubahan backend yang dibutuhkan.

**Dampak nyata.** Petugas memilih "Bekasi" yang pertama karena keduanya tampak sama. Bila ternyata salah,
daftar Kecamatan yang muncul adalah kecamatan wilayah lain, dan alamat pasien tersimpan pada kota yang
salah. Nama kembar Kota/Kabupaten juga lazim pada wilayah lain di Indonesia — misalnya Bogor, Bandung,
Malang, dan Kediri — sehingga masalah ini tidak hanya terjadi pada Bekasi. Query verifikasi untuk
menghitung seluruh nama kembar ada pada plan repair `FIX-EPS-003-02`.

**Rekomendasi.** Baca `additionalInfo` sebagai jenis kota sehingga label menjadi "Kabupaten Bekasi" dan
"Kota Bekasi". Perbaikan murni frontend. Lihat juga temuan tambahan `ISS-EPS-003-T1`, yang berakar di
berkas yang sama.

---

### ISS-EPS-003-03 — Butir 3 kosong pada laporan

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `PERLU_KONFIRMASI` |
| **Area** | — |
| **Keparahan** | Belum dapat ditetapkan |
| **Status bukti** | — |

Nomor 3 pada laporan tidak berisi teks. Dua lampiran — **lampiran 3** dan **lampiran 4** — tidak dirujuk
butir mana pun, sehingga besar kemungkinan butir 3 dimaksudkan untuk salah satunya.

Yang terlihat pada kedua lampiran itu dan **sudah** dicatat sebagai temuan tambahan: isian "Metode
Persalinan" yang tampil untuk semua pasien (`ISS-EPS-003-T3`). Yang terlihat tetapi **tidak** dianggap
masalah tanpa konfirmasi: isian Jenis Kelamin, Golongan Darah, Agama, dan Status Perkawinan berawal
"Tidak diketahui"; susunan isian Alamat; dan bagian Kontak Darurat.

Analisis butir lain tidak tertahan oleh butir ini. Lihat pertanyaan `P-01`.

---

### ISS-EPS-003-04 — Petugas diminta mengetik UUID

| | |
| --- | --- |
| **No. laporan** | 4 |
| **Jenis** | `RULE_VIOLATION` (aturan pelapor a dan b), `BUG` |
| **Area** | Frontend |
| **Keparahan** | High — satu isian pasti ditolak server; dua lainnya mustahil diisi petugas |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-003-04` |

**Apa yang terjadi.** Saat "Pasien Member" dicentang, muncul dua isian teks berlabel Inggris: "Default
Membership Tier ID" dengan petunjuk "UUID tier membership", dan "Active Patient Membership ID" dengan
petunjuk "UUID membership aktif". Saat "Pasien Bayi Baru Lahir" dicentang, muncul isian ketiga dengan pola
yang sama: "Patient ID Ibu" — "UUID pasien ibu". Petugas admisi tidak pernah melihat UUID di layar mana
pun, jadi tidak mungkin mengisinya dengan benar.

**Kenapa terjadi.** Ketiga isian adalah teks bebas — `new-patient-form.jsx:320-334` dan `:338-342`:

```javascript
<EmergencyTextField name="defaultMembershipTierId"
  label="Default Membership Tier ID" placeholder="UUID tier membership" />
<EmergencyTextField name="activePatientMembershipId"
  label="Active Patient Membership ID" placeholder="UUID membership aktif" />
// …
<EmergencyTextField name="motherPatientId"
  label="Patient ID Ibu" placeholder="UUID pasien ibu" />
```

**Akar.** Formulir diturunkan satu-banding-satu dari properti `CreatePatientRequest` —
`PatientDtos.cs:383-406` — yang bertipe `Guid?` untuk ketiganya. Properti bertipe `Guid` langsung
dijadikan isian teks tanpa disambungkan ke daftar pilihan; nama propertinya pun dipakai mentah sebagai
label.

Yang lebih parah, satu isian **pasti gagal** pada pasien baru. `PatientController.cs:2362-2369`:

```csharp
var normalizedActivePatientMembershipId = NormalizeNullableGuid(activePatientMembershipId);

if (normalizedActivePatientMembershipId.HasValue)
{
    if (!patientId.HasValue)
    {
        return (false, "Active patient membership hanya dapat dipilih setelah patient dibuat.");
    }
```

Membership aktif adalah kartu milik pasien yang sudah ada, dibuat lewat master Patient Membership.
Pasien yang sedang didaftarkan belum ada, sehingga isian itu tidak mungkin valid.

**Pola yang benar sudah ada.** Formulir master data pasien memakai daftar pilihan untuk dua dari tiga
isian ini — `patient-constants.jsx:109` (`defaultMembershipTierId`, *"Pilih membership tier"*) dan `:112`
(`motherPatientId`, *"Pilih pasien ibu"*). Backend juga sudah punya kedua sumber pilihannya:

| Kebutuhan | Endpoint yang ada | Catatan |
| --- | --- | --- |
| Tier membership | `GET /api/v1/administrator/master-data/membership-tiers/options` | Punya penyaring `isSelectableInAdmission`; butuh izin `MembershipTier : Read` (`MembershipTierController.cs:262`) |
| Pasien ibu | `GET /api/v1/health-services/patient-management/master-data/patients/options` | Kebijakan baca kiosk (`PatientController.cs:332-334`) |

**Apakah ini menyimpang dari desain?** Ya. Skema 3.3 tidak memuat isian-isian ini sama sekali (lihat
`ISS-EPS-003-06`), dan aturan pelapor menyatakan UUID tidak boleh diinput maupun ditampilkan.

**Dampak nyata.** Petugas mengisi sembarang teks atau mengosongkannya. Bila diisi, server menolak dan
seluruh simpan gagal dengan pesan berbahasa campur; bila dikosongkan, pasien tercatat member tanpa tier.

**Rekomendasi.** Hapus "Active Patient Membership ID" dari formulir pasien baru; ganti "Default
Membership Tier ID" menjadi daftar pilihan "Tier Membership" (nama + kode tier); ganti "Patient ID Ibu"
menjadi pencarian pasien (nama + No. RM). Rincian di `FIX-EPS-003-04`. Aturan pelapor diusulkan menjadi
aturan global — keputusan `K-03`.

---

### ISS-EPS-003-05 — Jam Lahir memakai kontrol bawaan peramban

| | |
| --- | --- |
| **No. laporan** | 5 |
| **Jenis** | `BUG` — tidak memakai base component yang tersedia |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-003-05` |

**Apa yang terjadi.** Isian Jam Lahir tampil sebagai kontrol jam bawaan peramban. Bentuknya berbeda dari
pemilih jam lain di Quilvian, dan pada sebagian pengaturan Windows tampil dalam format AM/PM.

**Kenapa terjadi.** `new-patient-form.jsx:365-369`:

```javascript
<EmergencyTextField name="birthTime" label="Jam Lahir" type="time" />
```

**Akar.** Pembungkus isian pendaftaran IGD (`emergency-registration-fields.jsx`) hanya menyediakan tiga
jenis: teks (baris 11), daftar pilihan (baris 15), dan tanggal (baris 78). Tidak ada pembungkus jam,
sehingga isian jam dibuat dari isian teks dengan `type="time"`.

Base component-nya sudah ada: `FilterTimePicker`
(`@/components/features/base-features/filter-time-picker`), tercatat pada katalog base component
dengan prop `format` 24 jam, `clearable`, dan `showNowButton`. IGD sendiri sudah memakainya di triase —
`emergency-triage-start-dialog.jsx:261`.

**Apakah ini menyimpang dari desain?** Ya — katalog base component mewajibkan reuse sebelum membuat
kontrol sendiri.

**Dampak nyata.** Kecil, karena Jam Lahir hanya tampil untuk bayi baru lahir. Namun petugas IGD yang
mendaftarkan bayi melihat dua gaya pemilih jam berbeda di sistem yang sama.

**Rekomendasi.** Tambahkan pembungkus `EmergencyTimeField` berbasis `FilterTimePicker` format 24 jam, lalu
pakai untuk Jam Lahir. Rincian di `FIX-EPS-003-05`.

---

### ISS-EPS-003-06 — Member, Bayi Baru Lahir, dan Meninggal dapat dicentang bersamaan

| | |
| --- | --- |
| **No. laporan** | 6 |
| **Jenis** | `BUG` — bagian ini tidak ada pada skema 3.3 |
| **Area** | Frontend |
| **Keparahan** | High — kombinasi yang dihasilkan ditolak server atau menghasilkan data yang tidak masuk akal |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-003-06` |

**Apa yang terjadi.** Bagian "Data tambahan pasien" memuat tiga kartu centang: Pasien Member, Pasien Bayi
Baru Lahir, dan Pasien Meninggal. Ketiganya dapat dicentang sekaligus (lampiran 1), sehingga petugas dapat
mendaftarkan "bayi baru lahir yang sudah meninggal dan menjadi member" untuk **dirawat inap**.

**Kenapa terjadi.** Ketiga kartu adalah `BaseCheckboxCard` yang berdiri sendiri, masing-masing terikat
ke satu boolean tanpa aturan hubungan — `new-patient-form.jsx:276-318`. `BaseCheckboxCard` memang kotak
centang, yaitu kontrol untuk pilihan yang boleh dipilih banyak sekaligus.

**Akar.** Ketiga hal itu sebenarnya **tidak sejenis**:

| Kartu | Sifat sebenarnya | Kontrol yang cocok |
| --- | --- | --- |
| Pasien Member | Atribut yang berdiri sendiri; boleh ada pada pasien jenis apa pun | Satu kotak centang atau sakelar |
| Bayi Baru Lahir | Kategori pasien — pada admisi rawat inap sudah dipilih di Langkah 1 Tipe Pasien | Tidak diinput lagi; diturunkan dari Langkah 1 |
| Pasien Meninggal | Status hidup — pada admisi justru tidak berlaku, karena pasien meninggal tidak dirawat inap | Tidak ditampilkan pada admisi |

Bagian ini terbawa karena `new-patient-form` IGD dipakai utuh. Skema 3.3 (`05-skema-tampilan.md:237-282`)
hanya memuat Identitas Pasien dan Kontak Darurat — **tidak ada** bagian Data tambahan.

**Bertentangan dengan dua keputusan yang sudah ada:**

1. Pendaftaran bayi baru lahir **belum masuk rilis ini** — keputusan pemilik 23-09-2026 pada
   `issue-002-admisi-bayi-baru-lahir-buntu.md`. Kartu "Bayi Baru Lahir" di Langkah 1 sudah dinonaktifkan
   (`inpatient-admission-flow-constants.jsx:202-213`), tetapi Langkah 2 tetap membolehkan menandai pasien
   sebagai bayi baru lahir. Satu pintu ditutup, pintu samping masih terbuka.
2. Skema tampilan bagian 27 (`05-skema-tampilan.md:1794`): *"Aturan pasien meninggal/kabur — tetap
   menunggu pemilik klinis dan tidak ditambahkan ke layar."*

**Aturan server yang tidak dicerminkan layar.** Server menolak kombinasi tertentu, tetapi layar tidak
memberi tahu sebelum Simpan ditekan:

| Kondisi | Penolakan server |
| --- | --- |
| Bayi Baru Lahir tanpa pasien ibu | *"Mother patient wajib dipilih untuk patient newborn."* — `PatientController.cs:2397` |
| Meninggal tanpa tanggal meninggal | *"Tanggal meninggal wajib diisi jika patient ditandai meninggal."* — `PatientController.cs:2164` |

**Dampak nyata.** Petugas yang iseng atau salah klik mencentang "Pasien Meninggal" lalu mengisi tanggal:
server menerimanya, dan pasien baru tercatat **meninggal** sebelum episode rawat inapnya dibuat.

**Rekomendasi.** Pisahkan menurut makna — hanya "Pasien Member" yang tetap berupa kartu centang (beserta
daftar pilihan Tier Membership); "Bayi Baru Lahir" diturunkan dari Langkah 1; "Pasien Meninggal" tidak
ditampilkan pada admisi. Kombinasi mustahil menjadi tidak mungkin dibuat, bukan sekadar dilarang. Opsi
lengkap dan skema tampilannya di plan repair `FIX-EPS-003-06`; keputusan `K-02`.

---

### ISS-EPS-003-07 — Tombol Simpan mati tanpa memberi tahu isian yang kurang

| | |
| --- | --- |
| **No. laporan** | 7 |
| **Jenis** | `DESIGN_CHANGE` — perilaku sekarang **sesuai** skema yang disetujui |
| **Area** | Frontend dan dokumen desain |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source dan dokumen |
| **Perbaikan** | `FIX-EPS-003-07`, `FIX-EPS-003-10` |

**Apa yang terjadi.** Tombol "Simpan & Lanjut ke Pembayaran" tampil pudar dan tidak dapat ditekan sampai
seluruh isian wajib terisi. Petugas tidak diberi tahu isian mana yang masih kosong; ia harus menyisir
sekitar lima belas isian di halaman yang panjang.

**Kenapa terjadi.** `inpatient-admission-registration-step.jsx:215-223` mematikan tombol dengan
`disabled={!canSubmitNewPatient}`. Nilai itu dihitung di
`use-inpatient-admission-patient.jsx:97-109` dari dua belas isian pada `REQUIRED_NEW_PATIENT_FIELDS`
ditambah tiga isian kontak darurat.

Formulir memakai `mode: "onBlur"` (baris 71-74), sehingga pesan merah hanya muncul pada isian yang pernah
disentuh. Isian yang belum pernah disentuh tetap tampak "bersih" walaupun wajib.

Padahal `handleSaveNewPatient` sudah memvalidasi ulang seluruh formulir (`await trigger()`, baris
195-197) — validasinya ada, hanya tidak pernah dicapai karena tombolnya mati, dan bila dicapai pun tidak
memindahkan fokus ke isian yang salah.

**Apakah ini menyimpang dari desain?** **Tidak.** Skema 3.3 (`05-skema-tampilan.md:274`) menetapkan
tombol ini aktif bila *"Seluruh isian wajib terisi; mati selama permintaan berjalan"*. Implementasinya
setia pada skema. Yang diminta pelapor adalah perubahan desain — karena itu skema 3.3 ikut direvisi
(`FIX-EPS-003-10`), agar agent berikutnya tidak "memperbaiki" tombolnya kembali menjadi mati.

Skema bagian 4.2 (`05-skema-tampilan.md:850`) juga menetapkan *"setelah error fokus pindah ke ringkasan
error"*, sedangkan pelapor meminta fokus pindah **ke isian** yang kosong. Rekomendasi kami memenuhi
keduanya: ringkasan tetap tampil, fokus masuk ke isian pertama — dengan revisi kalimat 4.2 untuk
formulir ini.

**Pembanding di repository yang sama:**

| Tempat | Perilaku |
| --- | --- |
| Pendaftaran IGD, `patient-selection-step.jsx:1573-1580` | Tombol "Simpan Pasien & Lanjut" hanya mati selama menyimpan — perilaku yang diminta pelapor |
| Resume rawat inap, `resume-form-panel.jsx:102-116` | Daftar isian yang salah, masing-masing dapat diklik; `scrollIntoView` lalu `focus()` ke isiannya — pola fokus yang bisa dipakai ulang |

**Hambatan teknis yang harus diperhatikan.** `EmergencySelectField` dan `EmergencyDateField`
(`emergency-registration-fields.jsx:35-65`, `:89-115`) memakai `Controller` tanpa meneruskan
`field.ref`. Fitur fokus otomatis bawaan react-hook-form karena itu **tidak dapat** memfokuskan daftar
pilihan dan tanggal; fokus harus dicari lewat elemen di halaman.

**Rekomendasi.** Tombol selalu dapat ditekan (kecuali selama menyimpan). Saat ditekan dengan isian belum
lengkap: tidak ada request ke server, ringkasan isian yang kurang tampil, halaman bergulir ke isian
pertama yang kosong, dan kursor masuk ke dalamnya. Rincian di `FIX-EPS-003-07`.

---

## 5. Tanya-jawab pelapor

> **T:** Tombol "Input Manual" ini untuk apa?
>
> **J:** Di admisi rawat inap tombol itu tidak berguna — menekannya tidak menjalankan apa pun. Tombol
> itu milik panel pemindai yang dirancang untuk alur yang menyembunyikan formulir sampai petugas memilih
> isi manual. Di admisi rawat inap formulirnya selalu tampil, jadi tombol itu tidak punya tugas dan
> memang layak dihilangkan. Detail: `ISS-EPS-003-01`.

> **T:** Kenapa "Bekasi" terlihat ganda?
>
> **J:** Karena label pilihan hanya memuat nama kota tanpa jenisnya. Datanya benar — Kabupaten Bekasi
> dan Kota Bekasi adalah dua wilayah berbeda, dan backend sudah mengirim jenisnya. Frontend saja yang
> membaca nama field yang salah (`cityType`, padahal backend mengirim `additionalInfo`), sehingga kata
> "Kota"/"Kabupaten" tidak pernah ditempel. Detail: `ISS-EPS-003-02`.

> **T:** Ini kenapa menginput UUID?
>
> **J:** Karena isian formulir dibuat langsung dari daftar properti API pembuatan pasien, dan ketiga
> properti itu bertipe UUID tanpa disambungkan ke daftar pilihan. Salah satunya, "Active Patient
> Membership ID", bahkan tidak mungkin benar untuk pasien baru — server selalu menolaknya karena
> membership aktif hanya bisa dipasang setelah pasiennya ada. Detail: `ISS-EPS-003-04`.

> **T:** Kenapa Jam Lahir tidak memakai base component?
>
> **J:** Karena kumpulan pembungkus isian pendaftaran IGD tidak punya pembungkus jam, sehingga isian jam
> dibuat dari isian teks bertipe `time`. Base component `FilterTimePicker` sudah tersedia dan sudah
> dipakai di triase IGD. Detail: `ISS-EPS-003-05`.

> **T:** Kenapa checkbox Data tambahan pasien bisa diklik semua?
>
> **J:** Karena ketiganya kotak centang mandiri tanpa aturan hubungan, padahal maknanya berbeda: Member
> adalah atribut, Bayi Baru Lahir adalah kategori yang sudah dipilih di Langkah 1, dan Meninggal adalah
> status yang tidak berlaku untuk admisi. Bagian ini juga tidak ada pada skema tampilan admisi — ia
> terbawa dari formulir IGD. Detail dan rekomendasi UI/UX: `ISS-EPS-003-06`.

> **T:** Kenapa tombol Simpan & Lanjut ke Pembayaran dimatikan?
>
> **J:** Karena skema tampilan yang disetujui memang memintanya: aktif hanya bila seluruh isian wajib
> terisi. Jadi ini bukan kesalahan pengerjaan, melainkan desain yang sekarang diminta diubah. Perubahan
> ini dicatat sebagai keputusan pemilik dan skema tampilannya ikut direvisi. Detail: `ISS-EPS-003-07`.

---

## 6. Temuan tambahan

Bagian ini **bukan** dari laporan pelapor. Semuanya ditemukan saat menelusuri butir di atas.

### ISS-EPS-003-T1 — Hasil scan KTP "Kota Bekasi" dapat terpasang ke "Kabupaten Bekasi"

| | |
| --- | --- |
| **Kenapa dicantumkan** | Berakar di berkas yang sama dengan `ISS-EPS-003-02`, dan lebih berbahaya karena tidak terlihat |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | High — data alamat salah tanpa disadari |
| **Status bukti** | SUDAH-VERIFIKASI pada logika pencocokan; `DUGAAN` pada pemenang seri (lihat cara membuktikan) |
| **Perbaikan** | `FIX-EPS-003-03` |

**Apa yang terjadi.** Saat KTP dipindai, sistem mencocokkan teks kota hasil OCR — misalnya "KOTA BEKASI"
— dengan master wilayah. Kata "KOTA" dan "KABUPATEN" **dibuang** sebelum dicocokkan, sehingga "Kota
Bekasi" dan "Kabupaten Bekasi" memperoleh nilai kecocokan yang persis sama.

**Kenapa terjadi.** `emergency-region.service.js:35-51` membuang kedua kata itu:

```javascript
.replace(/\bKABUPATEN\b/g, "")
.replace(/\bKAB\b/g, "")
.replace(/\bKOTA\b/g, "")
```

`scoreRegionOption` (baris 295-337) memberi nilai 1000 kepada keduanya, dan `findBestRegionOption`
(baris 339-358) mengambil yang **pertama** setelah diurutkan. Urutan "pertama" ditentukan backend, yang
hanya mengurutkan menurut nama (`RegionController.cs:1615`, `.OrderBy(x => x.CityName)`) — untuk nama
yang sama, urutannya tidak dijamin.

Kiosk sudah memecahkan masalah ini — `kiosk-new-patient-registration.helpers.jsx:788-793` menambah 300
bila jenis dari KTP cocok dan mengurangi 250 bila tidak cocok. Service IGD tidak.

**Cara membuktikan pemenang seri (`DUGAAN`).** Pindai satu KTP berdomisili Kota Bekasi pada admisi rawat
inap, lalu periksa isian Kota/Kabupaten dan daftar Kecamatan yang muncul. Bila Kecamatan berisi Cikarang
Utara atau Tambun Selatan, sistem memilih Kabupaten.

**Dampak nyata.** Petugas memercayai hasil scan, tidak memeriksa ulang wilayah, dan alamat pasien
tersimpan pada kabupaten yang salah. Kesalahan ini baru ketahuan bila surat atau kunjungan rumah salah
alamat.

### ISS-EPS-003-T2 — Pesan galat scanner tampil mentah "Failed to fetch"

| | |
| --- | --- |
| **Kenapa dicantumkan** | Terlihat pada lampiran 2, butir 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE |
| **Perbaikan** | `FIX-EPS-003-08` |

Saat Plustek Scanner Agent tidak berjalan, panel menampilkan *"Scanner belum dapat digunakan — Failed to
fetch"*. "Failed to fetch" adalah pesan teknis peramban berbahasa Inggris.

Hook sebenarnya sudah menyiapkan kalimat yang baik, tetapi tidak pernah dipakai —
`use-plustek-ktp-scanner.js:28-31`:

```javascript
const getErrorMessage = (error, fallback = "Proses scanner gagal.") =>
  toSafeString(
    error?.message || error?.body?.message || error?.detail || fallback,
  );
```

Pada baris 77-80 fallback-nya *"Plustek Scanner Agent tidak dapat diakses."*, tetapi kegagalan jaringan
selalu membawa `error.message` = "Failed to fetch", sehingga fallback tidak pernah terpakai. Hook ini
dipakai bersama IGD.

### ISS-EPS-003-T3 — Isian "Metode Persalinan" tampil untuk semua pasien

| | |
| --- | --- |
| **Kenapa dicantumkan** | Terlihat pada lampiran 3 yang tidak dirujuk butir mana pun; berkaitan dengan `ISS-EPS-003-06` |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE |
| **Perbaikan** | `FIX-EPS-003-09` |

Isian "Metode Persalinan" dengan petunjuk *"Opsional, khusus pasien bayi"* berada di bagian Identitas
Pasien (`new-patient-form.jsx:118-122`) dan tampil untuk setiap pasien, termasuk pasien dewasa. Data ini
milik data kelahiran bayi, sejajar dengan berat dan panjang lahir di bagian bayi baru lahir.

### ISS-EPS-003-T4 — Daftar pilihan Tier Membership di frontend menunjuk rute yang tidak ada

| | |
| --- | --- |
| **Kenapa dicantumkan** | `FIX-EPS-003-04` akan membutuhkan daftar tier; resource yang ada tidak dapat dipakai apa adanya |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend dan backend — modul `patient-management` |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Pemilik** | Modul Patient Management dan registry select frontend |

Resource `membershipTiers` pada `health-service-select-resources.js:337-343` memakai
`/v1/health-services/master-data/membership-tiers/options`. Rute itu **tidak ada** di backend; satu-satunya
controller tier adalah `MembershipTierController` pada `api/v1/administrator/master-data/membership-tiers`
(`MembershipTierController.cs:26`). Rute keliru itu bahkan diiklankan backend sendiri pada metadata filter
pasien — `PatientController.cs:126-131` dan `PatientMembershipController.cs:104`. Pemakai resource itu
akan mendapat 404.

### ISS-EPS-003-T5 — Master data pasien juga meminta UUID "Membership Aktif"

| | |
| --- | --- |
| **Kenapa dicantumkan** | Pelanggaran aturan pelapor yang sama dengan `ISS-EPS-003-04`, di modul lain |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend — modul `patient-management` |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Pemilik** | Modul Patient Management |

`patient-constants.jsx:110` mendefinisikan isian `activePatientMembershipId` berlabel "Membership Aktif"
dengan petunjuk *"ID membership aktif jika ada"* — teks bebas yang meminta UUID. Pada formulir ubah
pasien isian ini sah secara server, tetapi harus berupa daftar pilihan membership milik pasien itu.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Butir 3 kosong. Apa yang dimaksud? Apakah terkait lampiran 3 atau 4 — misalnya isian "Metode Persalinan" (`ISS-EPS-003-T3`)? | Dua lampiran tidak dirujuk butir mana pun | Butir 3 tidak dapat dianalisis; butir lain tetap berjalan |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Tombol Simpan selalu dapat ditekan dan mengarahkan ke isian yang kurang | — | **Sudah diambil** pelapor pada laporan 06-10-2026, butir 7. Dicatat; tidak ditanyakan ulang | — |
| K-02 | Isi bagian Data tambahan pasien pada admisi rawat inap | (a) Hanya Pasien Member + Tier Membership wajib + Catatan; (b) hapus seluruh bagian sesuai skema 3.3 lama; (c) tetap tiga kartu dengan aturan saling-kunci | **(a)** | `FIX-EPS-003-06`, `FIX-EPS-003-10` |
| K-03 | Aturan pelapor "UUID tidak boleh diinput dan tidak boleh tampil" dijadikan aturan global frontend | Ya, masuk `rules/frontend/ui-consistency-checklist.md` / Tidak, berlaku per layar | **Ya** | Tidak menahan perbaikan mana pun |
| K-04 | Bila peran petugas admisi belum punya izin baca Tier Membership | (a) Tambah izin `MembershipTier : Read` ke peran admisi; (b) backend menyediakan daftar tier berkebijakan baca registrasi, seperti `patients/options` | **(a)** bila izinnya memang sudah layak bagi admisi | `FIX-EPS-003-04` — hanya bila verifikasi izin gagal |

---

## 8. Catatan pola

**Pola 1 — komponen dipakai ulang tanpa disaring.** Tiga butir (`01`, `04`, `06`) dan satu temuan
tambahan (`T3`) lahir dari hal yang sama: komponen IGD dipakai utuh, termasuk bagian yang tidak tercantum
di skema 3.3. Skema 4.4 memang menetapkan *"Reuse dan komposisi"* untuk kerangka admisi — tetapi
komposisi berarti **memilih** bagian yang dipakai, bukan mengambil semuanya. Pencegahan: setiap kali
komponen modul lain dipakai ulang, builder membandingkan setiap wilayah yang tampil dengan skema layar
tujuan dan mencatat bagian yang disembunyikan.

**Pola 2 — formulir diturunkan langsung dari DTO.** Properti bertipe `Guid` menjadi isian teks, nama
properti menjadi label. Pola yang sama ada di master data pasien (`T5`). Pencegahan: keputusan `K-03`.

**Pola 3 — logika yang sama ditulis dua kali lalu menyimpang.** Kiosk dan IGD masing-masing punya
pencocok wilayah. Kiosk membaca `additionalInfo` dan membedakan Kota/Kabupaten; IGD tidak keduanya
(`02`, `T1`). Pencegahan: satukan penentuan label dan pencocokan wilayah dalam satu util bersama.

**Hubungan dengan issue terdahulu.** `ISSUE-EPS-002` menutup pendaftaran bayi baru lahir di Langkah 1.
Issue ini menemukan pintu samping yang masih terbuka di Langkah 2 (`06`). Perbaikan `FIX-EPS-003-06`
menutupnya.

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Issue dibuat dari laporan pemilik: 7 butir (1 perlu konfirmasi), 5 temuan tambahan, 4 keputusan | `diagnose-module-issue` |
