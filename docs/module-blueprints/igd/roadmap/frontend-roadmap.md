# Roadmap Delivery Frontend — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 2
wave: MVP-0
status: DRAFT
generated_at: "2026-08-24"
owners:
  - "Product/Domain Owner IGD — Rizki Gunawan (IGD-DEC-089)"
  - "Frontend authority untuk area DEV_DISCRETION (IGD-UI-004)"
approved_by: []
input_revisions:
  blueprint-manifest.md: 5
  03-frontend-architecture.md: 5
  04-prd-to-mvp.md: 5
contract_versions:
  - "State 0.3.0 — bagian 1, 1.1, 1.2 APPROVED (IGD-DEC-093)"
  - "Validation 0.3.0 — bagian 2 aturan 4-5 APPROVED (IGD-DEC-093)"
  - "API 0.3.0 — draft. TIDAK dipakai: gelombang ini nol perubahan endpoint"
artifact_hashes:
  03-frontend-architecture.md: "2b4339f9587ed1daff8444ccb68cb5415df578d76a2157dd3ec168f9a2a1fd95"
  04-prd-to-mvp.md: "7061525001d9a7e6b311424b8e3a8d85de13e35f59e545a78dcefedd600b79db"
  contracts/state-transition-matrix.md: "a41efd8d9adc87e1cf1eec2a9397b3521fdc0ebf935ccf0a19a5aa975b6c7c75"
  contracts/validation-matrix.md: "0ee98b750a29e01603db894ed3766614fe8989b2eef3573eab7d72cdc1a6b907"
source_commits:
  frontend: "96a9120111f6acc6b7c0f37973ea0c717ba41f17"
supersedes: "roadmap/archive/revision-1/frontend-roadmap.md"
```

---

## 0. Gelombang ini nyaris tidak menyentuh frontend

`EPIC IGD-03` adalah perbaikan perilaku backend. **Nol endpoint berubah, nol bentuk response
berubah, nol layar baru.** Yang berubah bagi petugas hanyalah: beberapa perbuatan yang dulu
diam-diam berhasil kini ditolak `409` beserta alasannya.

Karena itu gelombang ini hanya punya **satu** task frontend, dan sifatnya verifikasi.

### Yang sudah benar dan tidak perlu diubah

Diperiksa pada `96a91201`:

| Yang diperiksa | Hasil |
| --- | --- |
| `emergency-triage-form-view.jsx` baris 149 | Sudah punya `errorBanner` yang menampilkan `saveError` |
| `emergency-management-triage-slice.jsx` baris 279–281 | Jalur simpan sudah `catch` dan meneruskan `normalizeErrorMessage(error, …)` |

Pesan `409` dari backend karena itu **sudah punya jalan tampil**. Task di bawah membuktikannya
benar-benar tampil, bukan membangunnya dari nol.

---

## 1. Task

### `FE-IGD-012` — Penolakan `409` jalur triase tampil dengan pesan backend

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S01` |
| **Scope** | `emergency-triage-form-view.jsx`, `emergency-management-triage-slice.jsx`, dan tab penilaian ulang pada `emergency-assessment-view` |
| **Perubahan** | Diharapkan **nol atau nyaris nol**. Task ini memverifikasi; perubahan hanya ditulis bila verifikasi gagal |
| **Requirement** | `FR-IGD-013`, `FR-IGD-014` — sisi tampilan |
| **Kontrak** | Validation `0.3.0` bagian 2 aturan 4 dan 5 — hash `0ee98b75…` |
| **Dependency** | **`BE-IGD-019` selesai dan berjalan.** Sebelum itu penolakan yang harus ditampilkan belum ada |
| **Acceptance** | 1. Menyelesaikan triase pada kunjungan yang sudah ditutup menampilkan pesan backend apa adanya — *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* — **bukan** pesan cadangan *"Gagal menyimpan pemeriksaan triage."* 2. Penolakan transisi menampilkan pesan yang menyebut status kunjungan saat ini. 3. Tidak ada layar yang menampilkan status kunjungan sebagai `Triaged` setelah pasien `InTreatment` dinilai ulang. 4. Bila `normalizeErrorMessage` ternyata tidak membaca `response.data.message` untuk `409`, perbaiki **di tempat yang sudah ada**, jangan membuat penangan error tandingan |
| **Test** | `AT-IGD-086` sisi tampilan; `npm run lint` dan `npm test` tetap lulus |
| **Bukti** | Tangkapan layar kedua penolakan; catatan hasil untuk keempat butir acceptance |
| **Risiko** | Rendah |
| **Kewenangan UI** | **Tidak ada layar baru, tidak ada komponen baru, tidak ada CSS baru.** Bila butir 4 menuntut perubahan, ikuti `errorBanner` dan pola gaya yang sudah dipakai layar triase |
| **Owner** | Frontend |

---

## 2. Warisan revision `1` yang belum selesai

Task berikut **bukan** bagian `MVP-0`, tetapi belum dikerjakan dan tidak boleh hilang karena
pergantian revisi roadmap.

| Task | Isi | Keadaan | Catatan |
| --- | --- | --- | --- |
| `FE-IGD-010` | Halaman detail satu kunjungan IGD | **Belum dikerjakan.** Seluruh dependency-nya (`FE-IGD-004`, `007`, `008`, `009`) sudah selesai | Rincian penuh ada di `roadmap/archive/revision-1/frontend-roadmap.md` bagian `FE-IGD-010` |

`FE-IGD-010` dapat dikerjakan kapan saja dan **tidak** bergantung pada gelombang ini. Ia juga
tidak terpengaruh `IGD-DEC-091`: penggantian nama `emergency-transfers` menjadi
`emergency-departures` baru berlaku pada `MVP-3`, dan `FE-IGD-010` menampilkan data, bukan
memanggil route perpindahan.

`FE-IGD-001` sampai `FE-IGD-009` dan `FE-IGD-011` sudah selesai pada revision `1`.

---

## 3. Yang menunggu gelombang berikutnya

| Pekerjaan frontend | Menunggu | Sebabnya |
| --- | --- | --- |
| Mengganti `TRANSFER_URL` menjadi `emergency-departures` | `MVP-3` | `IGD-DEC-091`. Satu baris pada `emergency-assessment-slice.jsx:16`; **jangan** diubah sebelum backend-nya berganti |
| Layar dua rangkaian status kepergian | `MVP-3` | `IGD-DEC-090` |
| Layar koreksi dan pembalikan berpersetujuan | `MVP-3` | `IGD-DEC-090` |
| Serah terima SBAR dan sikap pesanan | `MVP-4` | `EPIC IGD-07` |
| Penanda unit tanpa kewenangan | `MVP-5` | `IGD-DEC-092`, dan pengesahan Security/Privacy owner |
| Layar pengkajian IGD tersimpan sungguhan | `POST-MVP` | Pemilik `ClinicalManagement` belum ditunjuk |

---

## 4. Definition of Done gelombang `MVP-0` — sisi frontend

| No | Butir | Bukti yang diterima |
| ---: | --- | --- |
| 1 | `FE-IGD-012` keempat butir acceptance-nya terjawab | Catatan hasil beserta tangkapan layar |
| 2 | `npm run lint` lulus | Keluaran perintah |
| 3 | `npm test` lulus | Keluaran perintah |
| 4 | Nol komponen, layar, atau modul CSS baru | Diff; bila kosong, sebutkan kosong |
| 5 | Alur simpan dijalankan sungguhan lewat layar | **Belum pernah terpenuhi** — butuh kredensial petugas. Bila masih belum ada, catat sebagai belum terbukti, jangan ditandai lulus |

Butir 5 adalah utang lama yang berlaku sejak revision `1` dan **tidak** diselesaikan gelombang
ini. Ia dicatat supaya tidak hilang, bukan supaya dianggap selesai.
