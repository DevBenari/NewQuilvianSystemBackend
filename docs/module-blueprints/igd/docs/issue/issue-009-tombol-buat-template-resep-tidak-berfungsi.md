# ISSUE-009 — Tombol "+ Buat Template Baru" pada Tab Template Resep Dokter IGD Tidak Berfungsi

```yaml
issue_id: ISSUE-IGD-009
module_id: igd
submodule: igd
layar: "Ruang Kerja Dokter IGD — Tab Resep (Sub-tab Template Resep)"
sumber_laporan: "Pertanyaan pengguna pada 18-11-2026 disertai 1 screenshot layar Ruang Kerja Dokter IGD"
tanggal_issue: "2026-11-18"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "ae054ee8 (rizkiG)"
source_sha_frontend: "a2ee4c22c (RizkiV2)"
rencana_perbaikan: ../plan-repair/plan-repair-009-tombol-buat-template-resep-tidak-berfungsi.md
ditulis_dengan: "skill diagnose-module-issue"
```

## 1. Ringkasan

Saat dokter berada di Ruang Kerja Dokter IGD, membuka menu/tab **Resep**, lalu memilih sub-tab **Template Resep**, terdapat tombol **"+ Buat Template Baru"** (baik pada header tabel maupun pada kotak status kosong/empty state). Ketika tombol tersebut diklik oleh dokter, sistem tidak memberikan respon, pesan, perpindahan layar, maupun aksi apa pun (*dead button*).

Penyebab langsungnya adalah komponen pemanggil `EmergencyDoctorPrescriptionTab` tidak meneruskan properti (*prop*) `onCreateNew` ke sub-komponen `PrescriptionTemplatesPanel`. Di dalam `PrescriptionTemplatesPanel`, pengendali klik tombol (`onClick={onCreateNew}`) memanggil nilai `undefined`, sehingga peramban (*browser*) tidak mengeksekusi tindakan apa pun.

Selain tombol yang tidak merespon, ditemukan ketidakcocokan nama parameter (*prop mismatch*) lain pada pemanggilan sub-komponen tersebut (`templatesState` alih-alih `state`, `loggedInDoctorName` alih-alih `doctorName`, dan `onRefresh` alih-alih `onRetry`), serta teks pemberitahuan pada sub-tab yang masih merujuk ke *"Ruang kerja rawat inap"*.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor (kata-kata pelapor) | Lampiran |
| :---: | --- | --- |
| 1 | "pada halaman ruang kerja, di menu resep atau tep resep lalu dibagian tab template resep kenapa kok saya klik buat template baru tidak terjadi action apa apa ya ?" | Lampiran 1: Screenshot Ruang Kerja Dokter IGD (dr. Jenah Ali, Pasien: Aldo Barbeto / RM 00-00-02-80 / Kunjungan IGD-261023035629-36B0C8), Sub-tab Template Resep menampilkan "Daftar Template Resep", tombol "+ Buat Template Baru", dan kotak "Belum Ada Template Resep" dengan tombol "+ Buat Template Baru". |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-IGD-009-01` | 1 | Tombol "+ Buat Template Baru" tidak mengeksekusi aksi apa pun saat diklik (*prop* `onCreateNew` tidak dioper) | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI; gejala DARI-CAPTURE | `FIX-IGD-009-01` |
| `ISS-IGD-009-T1` | — | Ketidakcocokan *prop* pada `PrescriptionTemplatesPanel` (`state`, `doctorName`, `onRetry`) melumpuhkan status memuat dan tombol coba lagi | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-IGD-009-01` |
| `ISS-IGD-009-T2` | — | Teks panduan template resep pada IGD masih menyebut *"Ruang kerja rawat inap..."* | `BUG` | Frontend | Low | SUDAH-VERIFIKASI; gejala DARI-CAPTURE | `FIX-IGD-009-02` |

---

## 4. Rincian per temuan

### ISS-IGD-009-01 — Tombol "+ Buat Template Baru" tidak berfungsi

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend — `emergency-doctor-prescription-tab.jsx` |
| **Keparahan** | Medium — dokter tidak dapat berpindah menyusun template resep dari sub-tab template |
| **Status bukti** | SUDAH-VERIFIKASI di source kode; gejala DARI-CAPTURE |
| **Perbaikan** | `FIX-IGD-009-01` |

**Apa yang terjadi.**
Ketika dokter mengeklik tombol **"+ Buat Template Baru"** (baik tombol di sudut kanan atas panel maupun tombol di bagian tengah layar saat template kosong), layar tetap diam dan tidak membuka formulir maupun beralih tab.

**Kenapa terjadi.**
Rantai penyebab:
```text
Tombol "+ Buat Template Baru" diklik dokter tetapi tidak bereaksi
  ← BaseButton menerima onClick={onCreateNew} bernilai undefined
    ← PrescriptionTemplatesPanel tidak menerima prop onCreateNew dari komponen induk
      ← emergency-doctor-prescription-tab.jsx merender PrescriptionTemplatesPanel tanpa onCreateNew
```

Bukti kode:
1. `QuilvianSystemFrontendDev/src/components/view/health-services/emergency-installation-management/doctor-emergency/tabs/emergency-doctor-prescription-tab.jsx` baris 137–147:
```jsx
{activeSegment === INPATIENT_PRESCRIPTION_SEGMENT.TEMPLATES ? (
  <PrescriptionTemplatesPanel
    templates={templates}
    templatesState={templatesState}
    loggedInDoctorName={loggedInDoctorName}
    onApplyTemplate={applyTemplateToDraft}
    onDeleteTemplate={deleteTemplate}
    onRefresh={refreshTemplates}
    canWrite={canWrite}
  />
) : null}
```

2. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription/prescription-templates-panel.jsx` baris 32–41 & 72–84:
```jsx
export default function PrescriptionTemplatesPanel({
  templates = [],
  state,
  canWrite = false,
  doctorName = "",
  onApplyTemplate,
  onDeleteTemplate,
  onCreateNew,
  onRetry,
}) {
  ...
  const createButton = (
    <ClinicalActionGuard allowed={canWrite} mode="hide">
      <BaseButton
        type="button"
        variant="primary"
        size="sm"
        onClick={onCreateNew}
        data-testid="create-template-button"
      >
        ＋ Buat Template Baru
      </BaseButton>
    </ClinicalActionGuard>
  );
```

**Bagaimana konsep pembuatan template resep di sistem Quilvian?**
Di arsitektur resep klinis Quilvian (baik Rawat Inap maupun IGD), template resep **tidak dibuat secara kosong dari formulir terpisah**, melainkan **dibentuk langsung dari draf resep aktif (obat tunggal atau racikan) yang disusun di tab "Buat Resep"**.

Oleh karena itu:
- Pada sub-tab **"Template Resep"**, tombol "+ Buat Template Baru" berfungsi **mengalihkan navigasi aktif dokter kembali ke sub-tab "Buat Resep"** (`setActiveSegment(INPATIENT_PRESCRIPTION_SEGMENT.BUILDER)`).
- Dokter kemudian menyusun daftar obat dan racikan yang diinginkan di draf resep.
- Dokter menekan tombol **"Simpan sebagai Template"** di bawah draf resep, yang membuka modal input (*dialog*) `DoctorPrescriptionTemplateCreateModal` untuk mengisi Nama Template, Kategori, Deskripsi, dan Favorit.

Sebagai bukti pembanding yang sudah berjalan dengan benar:
- Pada Rawat Inap (`inpatient-prescription-tab.jsx` baris 118–120 & 185):
  `onCreateNew={openBuilderForNewTemplate}` di mana `openBuilderForNewTemplate = () => setActiveSegment(INPATIENT_PRESCRIPTION_SEGMENT.BUILDER);`
- Pada Asesmen IGD (`emergency-assessment-prescription-tab.jsx` baris 262–264):
  `onCreateNew={() => setActiveSegment(INPATIENT_PRESCRIPTION_SEGMENT.BUILDER)}`

---

### ISS-IGD-009-T1 — Ketidakcocokan *prop* pada `PrescriptionTemplatesPanel`

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `BUG` |
| **Area** | Frontend — `emergency-doctor-prescription-tab.jsx` |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source kode |
| **Perbaikan** | `FIX-IGD-009-01` |

**Kenapa terjadi.**
Terdapat perbedaan penamaan *prop* antara apa yang dikirim oleh `emergency-doctor-prescription-tab.jsx` dengan yang diterima oleh `PrescriptionTemplatesPanel.jsx`:

1. `templatesState={templatesState}` vs `state`:
   `PrescriptionTemplatesPanel` menggunakan `state?.loading`, `state?.error`, `state?.denied` pada batas kendali status `ClinicalStateBoundary`. Karena yang dikirim adalah `templatesState`, `state` selalu bernilai `undefined`, sehingga status *loading spinner* dan pesan galat saat gagal mengambil data template tidak muncul sebagaimana mestinya.
2. `loggedInDoctorName={loggedInDoctorName}` vs `doctorName`:
   `PrescriptionTemplatesPanel` menampilkan nama dokter pada judul panel `Daftar Template Resep (dr. Jenah Ali)`. Karena dioper dengan nama `loggedInDoctorName`, parameter `doctorName` bernilai kosong.
3. `onRefresh={refreshTemplates}` vs `onRetry`:
   `PrescriptionTemplatesPanel` menghubungkan tombol "Coba Lagi" saat galat ke `onRetry`. Karena dioper sebagai `onRefresh`, tombol "Coba Lagi" tidak melakukan apa-apa saat diklik.

---

### ISS-IGD-009-T2 — Teks panduan template resep pada IGD masih menyebut *"Ruang kerja rawat inap..."*

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan) |
| **Jenis** | `BUG` |
| **Area** | Frontend — `prescription-templates-panel.jsx` / konstanta |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source kode & terlihat di capture pelapor |
| **Perbaikan** | `FIX-IGD-009-02` |

**Kenapa terjadi.**
Pada baris 99 `prescription-templates-panel.jsx`:
```jsx
<p className={styles.formNotice}>{INPATIENT_PRESCRIPTION_TEXT.templateMineOnlyNotice}</p>
```
Konstanta `INPATIENT_PRESCRIPTION_TEXT.templateMineOnlyNotice` bernilai:
`"Ruang kerja rawat inap hanya menampilkan template pribadi milik Anda sendiri."`

Komponen ini digunakan bersama antara Rawat Inap dan Ruang Kerja Dokter IGD. Saat ditampilkan di IGD, penyebutan "rawat inap" membingungkan dokter IGD karena pasien sedang berada di instalasi gawat darurat. Perlu disediakan *prop* teks pemberitahuan yang fleksibel (*noticeText*) dengan bawaan netral atau disesuaikan dengan konteks IGD.

---

## 5. Tanya-jawab pelapor

| No | Pertanyaan pelapor | Jawaban |
| :---: | --- | --- |
| 1 | "kenapa kok saya klik buat template baru tidak terjadi action apa apa ya ?" | Tombol tersebut belum disambungkan ke fungsi navigasi pembangun resep (*prop `onCreateNew` tidak diteruskan*). Di sistem Quilvian, template resep tidak dibuat dari form kosong tersendiri, melainkan disusun dari obat dan racikan di draf tab **"Buat Resep"**, lalu disimpan menggunakan tombol **"Simpan sebagai Template"**. Ketika tombol "+ Buat Template Baru" diklik, seharusnya sistem langsung memindahkan tab aktif Anda ke tab **"Buat Resep"**. |

---

## 6. Temuan tambahan

Lihat subbagian `ISS-IGD-009-T1` (ketidakcocokan nama properti) dan `ISS-IGD-009-T2` (teks panduan rawat inap di ruang IGD). Keduanya dapat diselesaikan bersamaan dalam satu perbaikan berkas frontend tanpa menyentuh backend.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

| No | Untuk | Pertanyaan | Rekomendasi |
| :---: | --- | --- | --- |
| K-01 | Pengembang / Pemilik | Apakah tombol "+ Buat Template Baru" cukup mengarahkan dokter kembali ke sub-tab "Buat Resep" seperti halnya pada Rawat Inap dan Asesmen IGD? | **Ya (Rekomendasi)**: Sambungkan `onCreateNew={() => setActiveSegment(INPATIENT_PRESCRIPTION_SEGMENT.BUILDER)}`. Alur ini konsisten dengan arsitektur resep di seluruh modul Quilvian. |
| K-02 | Pengembang / Pemilik | Apakah teks panduan di bawah judul template resep perlu dibuat umum agar tidak menyebut "rawat inap" saat dibuka di IGD? | **Ya (Rekomendasi)**: Sediakan opsi *prop* `noticeText` pada `PrescriptionTemplatesPanel` sehingga modul IGD dapat menampilkan kalimat netral seperti: *"Halaman ini hanya menampilkan template resep pribadi milik Anda sendiri."* |

---

## 8. Catatan pola

- **Penggunaan Ulang Komponen Lintas Modul Tanpa Verifikasi Prop**: Komponen `PrescriptionTemplatesPanel` berasal dari paket Rawat Inap (`physician-workspace/tabs/prescription`). Ketika diimpor ke modul IGD (`doctor-emergency/tabs/emergency-doctor-prescription-tab.jsx`), parameter prop tidak disinkronkan dengan kontrak komponen aslinya.
- **Pola Tombol Tanpa Aksi (Dead Control)**: Setiap tombol aksi penting wajib memiliki penanganan fallback atau peringatan konsol pada mode pengembangan jika fungsi pengendali tidak dioper.

---

## 9. Riwayat dokumen

| Tanggal | Penulis | Perubahan |
| --- | --- | --- |
| 2026-11-18 | Antigravity AI | Pembuatan dokumen issue awal berdasarkan pertanyaan pengguna dan bukti source code. |
