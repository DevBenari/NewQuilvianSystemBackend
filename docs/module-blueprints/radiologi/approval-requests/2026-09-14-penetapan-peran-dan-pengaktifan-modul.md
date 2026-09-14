# Penetapan Peran dan Langkah Pengaktifan Modul Radiologi

| Field | Nilai |
|---|---|
| Tanggal | 2026-09-14 |
| Diputuskan | **Yoga Aji Pratama**, pemilik modul, atas nama Komite Medis |
| Menutup | `RAD-OPEN-011` |
| Membuka | `RAD-OPEN-012` |
| Keputusan yang lahir | `RAD-DEC-017` |
| Status | Keputusan **disetujui**; pelaksanaannya **belum dijalankan** — lihat bagian 3 |

---

## 1. Yang diputuskan

| Kewenangan | Pemegang | Catatan |
|---|---|---|
| `RadSafetyRule : Approve`, `: Reject`, `: Deactivate` | **Yoga Aji Pratama** | Menjalankan kewenangan Komite Medis sehari-hari — `RAD-DEC-016` |
| `RadReport : ActAsRadiologist` | Posisi **Dokter Radiologi** | **Sengaja tidak melekat pada pemegang akun tata kelola.** Mengesahkan aturan keselamatan dan dihitung sebagai dokter radiolog adalah dua kewenangan berbeda |

**Mengapa keduanya dipisah.** Menggabungkannya berarti satu orang menetapkan aturan keselamatan
*dan* membaca hasil atas nama radiolog. Pemisahan ini yang membuat `RAD-DEC-003` dan
`RAD-DEC-005` punya arti; menggabungkannya akan menghapus keduanya sekaligus tanpa ada yang
menyadarinya.

---

## 2. Satu akibat yang perlu diketahui sebelum melangkah

**Menunjuk satu pengesah saja belum membuat modul dapat berjalan.**

`RadSafetyPolicyService.ApproveAsync` menegakkan `AC-13`:

```csharp
if (actorUserId == rule.SubmittedByUserId || actorUserId == rule.CreateBy)
    → 403 "Aturan yang Anda susun atau ajukan sendiri harus disahkan
           penanggung jawab klinis lain."
```

Yang diperiksa **dua**: penyusun dan pengaju. Keterangan pada kodenya menyebut alasannya —
memeriksa pengaju saja menyisakan jalan memutar paling mudah, yaitu menyusun draf lalu meminta
orang lain sekadar menekan tombol *Ajukan*.

**Akibatnya bagi dua belas draf aturan yang sudah ada:**

| Ruas | Nilai | Akibat |
|---|---|---|
| `CreateBy` | `Guid.Empty` — diisi seeder `BE-RAD-15`, bukan orang | Yoga **tidak** terhalang sebagai penyusun |
| `SubmittedByUserId` | Belum terisi | **Siapa pun yang menekan *Ajukan* menjadi terhalang mengesahkannya** |

Jadi langkah *Ajukan* wajib dijalankan **orang lain**, bukan Yoga. Modul ini menuntut **dua
orang** pada gerbang keselamatannya, dan itu memang rancangannya — bukan hambatan yang perlu
dicari jalan pintasnya.

Dicatat sebagai **`RAD-OPEN-012`**: siapa yang mengajukan aturan keselamatan.

---

## 3. Langkah pengaktifan — berurutan, dan tidak dapat ditukar

### Langkah 1 — Administrator: pastikan hak akses tersemai

Jalankan aplikasi sekali. `AccessMenuSeeder` mendaftarkan seluruh pasangan hak akses, termasuk
penanda tanpa endpoint `RadReport : ActAsRadiologist`.

**Yang diperiksa sesudahnya:** baris `RadReport` / `ActAsRadiologist` muncul pada layar Akses
Role. Bila tidak muncul, berhenti — langkah berikutnya tidak akan berhasil.

### Langkah 2 — Administrator: berikan kewenangan

Hak akses Quilvian melekat pada pasangan **Department + Position**, bukan pada nama peran.
Jadi yang diberikan adalah kebijakan akses untuk posisi yang bersangkutan.

| Yang diberikan | Kepada | Untuk |
|---|---|---|
| `RadSafetyRule : Approve`, `: Reject`, `: Deactivate` | Posisi yang ditempati **Yoga Aji Pratama** | Mengesahkan aturan keselamatan |
| `RadReport : ActAsRadiologist` | Posisi **Dokter Radiologi** | Menulis dan mengesahkan hasil bacaan sebagai radiolog |
| `RadReport : Validate`, `: Release` | Posisi **Dokter Radiologi** | Menjalankan pengesahan dan perilisan |

> **`ActAsRadiologist` tidak boleh masuk paket hak akses umum.** `RAD-PERM-001` bagian 6
> menyatakannya eksplisit. Ia menjawab "dihitung sebagai dokter radiolog", dan memberikannya
> kepada posisi yang bukan dokter radiolog akan meruntuhkan `RAD-DEC-003` tanpa satu pun galat
> yang terlihat.

### Langkah 3 — Radiologi: ajukan dua belas draf aturan

Layar **Aturan Keselamatan Radiologi** → tombol **Ajukan** pada setiap draf.

**Dijalankan orang selain Yoga Aji Pratama** — lihat bagian 2. Pengaju menjadi terhalang
mengesahkan aturan yang sama.

Dua belas draf itu sudah disiapkan `BE-RAD-15` untuk enam alat: X-Ray `CR`, CT-Scan `CT`,
MRI `MR`, USG `US`, Mamografi `MG`, dan Fluoroskopi `RF`.

### Langkah 4 — Yoga Aji Pratama: sahkan

Layar yang sama → tombol **Sahkan**. Statusnya berpindah `PendingApproval` → `Active`,
`RuleVersion` naik satu, dan `ApprovedByUserId` beserta `ApprovedAt` terisi.

**Sesudah langkah ini gerbang keselamatan mulai meloloskan pemeriksaan.** Sebelum ini, setiap
pemeriksaan pada alat mana pun ditolak — dan itu memang perilaku yang benar.

### Langkah 5 — Verifikasi

| Yang diperiksa | Cara | Hasil yang diharapkan |
|---|---|---|
| Aturan sudah berlaku | `GET /rad-safety-rules/coverage` | Tidak ada alat yang tercatat tanpa aturan aktif |
| Gerbang meloloskan | Jalankan satu pemeriksaan sampai `SafetyCleared` | Tidak lagi ditolak "Aturan keselamatan untuk modalitas ini belum ditetapkan" |
| Bacaan lahir sendiri | Nilai mutu satu citra sebagai **layak** | Satu baris `RadReport` berstatus `Pending` muncul pada daftar bacaan menunggu — **perilaku baru dari `BE-RAD-16`** |
| Pengesahan bacaan | Sahkan satu draf bacaan sebagai Dokter Radiologi | Tidak lagi ditolak `403 RAD_VALIDATOR_NOT_RADIOLOGIST` |

---

## 4. Yang sengaja **tidak** dibuatkan jalan pintas

| Usulan yang ditolak | Alasan |
|---|---|
| Seeder yang menerbitkan aturan berstatus `Active` | Berarti sebuah program menetapkan kapan seorang pasien aman disinari. Meruntuhkan `RAD-DEC-002` dan `RAD-DEC-005` sekaligus. Sudah ditolak `BE-RAD-15`, dan ditolak lagi di sini |
| Skrip SQL yang menyetel `RuleStatus = Active` | Melewati `ApproveAsync`, sehingga `ApprovedByUserId`, `ApprovedAt`, kenaikan `RuleVersion`, dan pemeriksaan tabrakan aturan aktif **seluruhnya terlewat**. Yang tercatat kemudian adalah aturan berlaku yang tidak diketahui siapa yang mengesahkannya |
| Melonggarkan `AC-13` agar satu orang cukup | Pemisahan penyusun dan pengesah **adalah** pengamannya. Melonggarkannya berarti menghapus alasan gerbang ini ada |

---

## 5. Keadaan setelah dokumen ini

| Hal | Keadaan |
|---|---|
| `RAD-OPEN-011` | **Ditutup** — `RAD-DEC-017` |
| `RAD-OPEN-012` | **Dibuka** — siapa yang mengajukan aturan keselamatan |
| Kode backend | **Selesai.** `BE-RAD-16` menutup kelahiran bacaan otomatis; build 0 error |
| Kode frontend | **Selesai.** `FE-RAD-01` sampai `FE-RAD-13`; 920 unit test lulus |
| Pelaksanaan langkah 1–5 | **Belum dijalankan.** Menuntut sistem berjalan, sesi login, dan dua orang berbeda |

**Tidak ada satu pun langkah pada bagian 3 yang dijalankan agent.** Seluruhnya menuntut sesi
pengguna sungguhan, dan langkah 3 serta 4 menuntut dua orang yang berbeda — pengesahan aturan
keselamatan adalah tindakan klinis, bukan tindakan teknis.
