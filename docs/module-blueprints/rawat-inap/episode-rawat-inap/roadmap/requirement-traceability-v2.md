# Requirement Traceability V2 — Sub-modul Episode Rawat Inap

> Berkas ini **baru** dan hanya melacak amandemen terbatas revision `7`
> (`PRD-RWI-V2-001`). Traceability task lama `BE-RWI-001` s.d. `BE-RWI-078` dan
> `FE-RWI-001` s.d. `FE-RWI-062` tetap di [`requirement-traceability.md`](./requirement-traceability.md).

## Metadata

```yaml
blueprint_id: RWI-BP-001
blueprint_revision: 7
submodule: episode-rawat-inap
traceability_revision: 1
contract_version: 0.9.0
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
approved_by: "Muhammad Hamzah"
approved_at: "2026-09-16"
upstream_input: "PRD-RWI-V2-001 v2.0"
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
backend_roadmap: roadmap/backend-roadmap-v2.md
frontend_roadmap: roadmap/frontend-roadmap-v2.md
```

---

## 1. Requirement → desain → kontrak → task → bukti

| FR | Epic | Keputusan | Desain | Kontrak `0.9.0` | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FR-RI-191` | `RI-38` | `RWI-DEC-117` | `02-backend` 11 | API 10.1 | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] | Acceptance 18.1; `UAT-45` | Belum dikerjakan |
| `FR-RI-192` | `RI-38` | `VAL-INP-11` | `02-backend` 11 | API 10.1 | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] | Acceptance 18.1 | Belum dikerjakan |
| `FR-RI-193` | `RI-39` | `RWI-DEC-099` | `02-backend` 11.8 E1 | API 10.2 | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` | Acceptance 18.2; `UAT-46` | Belum dikerjakan |
| `FR-RI-194` | `RI-39` | `RWI-DEC-130`; `INV-INP-12` | `02-backend` 11.8 E1 | `VAL-INP-01`, `08` | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` | Acceptance 18.2; `UAT-46`, `UAT-47` | Belum dikerjakan |
| `FR-RI-195` | `RI-39` | `VAL-INP-09` | `02-backend` 11 | API 10.2 | `BE-RWI-080` | `FE-RWI-063` | Acceptance 18.2; `UAT-48` | Belum dikerjakan |
| `FR-RI-196` | `RI-40` | `RWI-DEC-112` | `data-dictionary` 18.2–18.3 | data 18.2–18.3 | `BE-RWI-085` | `FE-RWI-064` | Acceptance 18.3; `UAT-49` | Belum dikerjakan |
| `FR-RI-197` | `RI-40` | `NFR-027` | `02-backend` 11 | API 10.3 | `BE-RWI-086` | `FE-RWI-064` | Acceptance 18.3; `UAT-49` | Belum dikerjakan |
| `FR-RI-198` | `RI-41` | `RWI-DEC-138`; `RM-DEC-003` | `02-backend` 11.8 E3 langkah 4 | `INT-INP-08` | `BE-RWI-082` | `FE-RWI-065` | Acceptance 18.4; `UAT-50` | Belum dikerjakan |
| `FR-RI-199` | `RI-41` | `RWI-DEC-143` | `02-backend` 11.8 E3 langkah 5 | `INT-INP-09`; API 10.4 | `BE-RWI-083` | `FE-RWI-065`, `FE-RWI-066` | Acceptance 18.4; `UAT-50` | Belum dikerjakan |
| `FR-RI-200` | `RI-41` | `RWI-DEC-143`; `INT-KEP-15` | `02-backend` 11.8 E3 langkah 6 | `INT-INP-10` | `BE-RWI-087` | `FE-RWI-065` | Acceptance 18.4; `UAT-50` | Belum dikerjakan |
| `FR-RI-201` | `RI-41` | `VAL-INP-13` s.d. `17` | `03-frontend` 12 | validation matrix | `BE-RWI-084` | `FE-RWI-065` | Acceptance 18.4; `UAT-51` | Belum dikerjakan |

**Sebelas FR, sebelas baris, nol FR tanpa task, nol FR tanpa bukti acceptance.**

---

## 2. Kebutuhan non-fungsional

| NFR | Bunyi | Task | Cara membuktikan | Status |
| --- | --- | --- | --- | --- |
| `NFR-025` | Penutupan beserta akibatnya atomik pada PostgreSQL sungguhan | `BE-RWI-082`, `BE-RWI-083`, `BE-RWI-084`, `BE-RWI-087` | Galat buatan tiap langkah → nol perubahan, dijalankan pada container Postgres sekali pakai | Belum dikerjakan |
| `NFR-026` | Census `assignedToMe` memakai index penugasan per dokter; waktu aktif dibaca saat query tanpa proses latar | `BE-RWI-081` | Rencana eksekusi memakai index; uji batas 06.59 dan 07.01 | Belum dikerjakan |
| `NFR-027` | Usulan isian resume selesai paling lama 5 detik per sumber — **angka usulan desain** | `BE-RWI-086` | Pengukuran dicatat pada laporan task; angka nyata dilaporkan apa adanya | Belum dikerjakan |

---

## 3. Skenario UAT dan task yang menanggungnya

| UAT | Epic | Jalur | Task BE | Task FE |
| --- | --- | --- | --- | --- |
| `UAT-45` | `RI-38` | Berhasil | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] |
| `UAT-46` | `RI-39` | Berhasil | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` |
| `UAT-47` | `RI-39` | Gagal | `BE-RWI-080` | `FE-RWI-063` |
| `UAT-48` | `RI-39` | Gagal | `BE-RWI-080` | `FE-RWI-063` |
| `UAT-49` | `RI-40` | Berhasil | `BE-RWI-085`, `BE-RWI-086` | `FE-RWI-064` |
| `UAT-50` | `RI-41` | Berhasil | `BE-RWI-082`, `083`, `084`, `087` | `FE-RWI-065` |
| `UAT-51` | `RI-41` | Gagal | `BE-RWI-084` | `FE-RWI-065` |

---

## 4. Dependency yang keluar dari sub-modul ini

| Task di sini | Menunggu | Milik | Kenapa |
| --- | --- | --- | --- |
| `BE-RWI-082` | `BE-RWI-091` | `dokter-rawat-inap` | Penguncian konsep butuh registrasi keutuhan sejak konsep (`R2`) |
| `BE-RWI-083` | `BE-RWI-097` | `dokter-rawat-inap` | Pembatalan pesanan butuh `PatientProcedureOrderService` (`R7`) |
| `BE-RWI-087` | `BE-RWI-114` | `keperawatan` | Pembatalan dosis butuh tabel MAR (`K4`) |

## 5. Dependency yang masuk ke sub-modul ini

| Task di luar | Menunggu | Kenapa |
| --- | --- | --- |
| `BE-RWI-091` s.d. `BE-RWI-098` (`dokter-rawat-inap`) | `BE-RWI-079` | Penjaga penulis klinis membaca tujuan penugasan — `02-module-map.md` 3.4.1 gelombang V1 |
| `FE-RWI-067` (`dokter-rawat-inap`) | `BE-RWI-081` | Daftar pasien ruang kerja dokter memakai census `assignedToMe` |
| `FE-RWI-074` (`dokter-rawat-inap`) | `BE-RWI-085`, `BE-RWI-086` | Tab Resume Medis memakai kontrak resume yang sama |

---

## 6. Gerbang yang belum tertutup

| Gerbang | Jenis | Menahan apa | Pemilik |
| --- | --- | --- | --- |
| ~~Pemberitahuan `INT-INP-08`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-151` | ~~DoD `BE-RWI-082`~~ — cukup merujuk keputusan itu | Yoga Aji Pratama ✅ |
| Nasib pesanan tertagih — 22.7 nomor 1 | Keputusan produk | Tidak menahan; menilai ulang cakupan `BE-RWI-083` dan `FE-RWI-066` | Muhammad Hamzah + pemilik Billing |
| Isi minimal resume sebelum tanda tangan — 22.7 nomor 3 | Gerbang produksi | Pemakaian pada pasien sungguhan | Pemilik klinis, **belum ditunjuk** |
| ~~Persetujuan Yoga Aji Pratama atas pemanggil penguncian — 22.7 nomor 4~~ | **TERTUTUP 2026-09-16** `RWI-DEC-151` | ~~Rilis `RI-V2-1`~~ — bebas | Yoga Aji Pratama ✅ |

---

## 7. Catatan revision

| Revision | Tanggal | Isi |
| ---: | --- | --- |
| `1` | 2026-09-16 | Dibuat `plan-module-delivery` fase `RLN-PH-07` sesudah `RWI-DEC-150`. Sebelas FR amandemen terbatas revision `7` dipetakan ke sembilan task backend dan empat task frontend. Ditulis sebagai berkas terpisah atas permintaan pemilik agar berkas lama tetap terbaca |
